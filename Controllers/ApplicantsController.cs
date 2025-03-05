using Hiring_Application.Data;
using Hiring_Application.Models;
using Hiring_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


namespace Hiring_Application.Controllers
{
    [Authorize]
    public class ApplicantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly EmailService _emailService;


        public ApplicantsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, EmailService emailService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _emailService = emailService;
        }


        // GET: Applicant
        [Authorize(Roles = "HR,Admin")]

        public async Task<IActionResult> Index(string search,string status, int jobId, int page = 1)
        {
            // Pagination
            int pageSize = 5;
            IQueryable<Applicant> query = _context.Applicants.Include(a => a.Job);

            // filtering by Name
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.FirstName.Contains(search) || a.LastName.Contains(search));
            }

            // Filtering by Status
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(status, out ApplicantStatus appStatus))
                {
                    query = query.Where(a => a.Status == appStatus);
                }
            }
            if (jobId > 0)
            {
                query = query.Where(a => a.JobId == jobId);
            }

            var applicants = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Jobs = _context.Jobs.ToList();
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.JobId = jobId;
            ViewBag.CurrentPage = page;
            int totalApplicants = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalApplicants / (double)pageSize);
            return View(applicants);
        }


        // GET: Create
        [Authorize(Roles = "Applicant,Admin")]
        public IActionResult Create()
        {
            ViewBag.Jobs = new SelectList(_context.Jobs, "JobId", "Title"); // Assuming you have a Jobs table
            return View();
        }

        // POST: Create
        [Authorize(Roles = "Applicant,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Applicant applicant, IFormFile? file)
        {

            if (!ModelState.IsValid)
            {
                return View(applicant);
            }

            applicant.ApplicantId = GenerateApplicantId();

            try
            {
                if (file != null && file.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var allowedExtensions = new[] { ".pdf", ".docx" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("", "Only PDF or DOCX files are allowed.");
                        return View(applicant);
                    }

                    string safeFirstName = Regex.Replace(applicant.FirstName, @"[^a-zA-Z0-9]", "");
                    string safeLastName = Regex.Replace(applicant.LastName, @"[^a-zA-Z0-9]", "");

                    string fileName = $"{safeFirstName}_{safeLastName}_cv{fileExtension}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    applicant.ResumePath = "/Uploads/" + fileName; 
                }

                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync();

                await _emailService.SendApplicationCreatedEmailAsync(applicant.Email, applicant.FirstName + " " + applicant.LastName, applicant.ApplicantId);

                TempData["SuccessMessage"] = "Application submitted successfully! Check your email for confirmation.";
                return RedirectToAction("ApplicantDashboard", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
            }
            ViewBag.Jobs = new SelectList(_context.Jobs, "JobId", "Title", applicant.JobId);

            return View(applicant); 
        }

        private async Task SendApplicationEmail(string email, int applicantId, string firstName)
        {
            await Task.CompletedTask;
        }



        // GET: Edit
        [Authorize(Roles = "Applicant,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                TempData["ErrorMessage"] = "Application not found!";
                return RedirectToAction("EditApplication");
            }

            ViewBag.Jobs = _context.Jobs
                .Select(j => new SelectListItem
                {
                    Value = j.JobId.ToString(),
                    Text = j.Title
                }).ToList();

            return View(applicant);
        }

        [Authorize(Roles = "Applicant,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Applicant model, IFormFile? file)
        {
            if (id != model.ApplicantId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingApplicant = await _context.Applicants.AsNoTracking().FirstOrDefaultAsync(a => a.ApplicantId == id);

                    if (existingApplicant == null)
                    {
                        return NotFound();
                    }

                    if (file == null)
                    {
                        model.ResumePath = existingApplicant.ResumePath; 
                    }
                    else
                    {
                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string fileName = $"{model.FirstName}_{model.LastName}_CV.pdf";

                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        model.ResumePath = "/Uploads/" + fileName; 
                    }

                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Applicant updated successfully!";
                    return RedirectToAction("ApplicantDashboard", "Home");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Something went wrong. Please try again.");
                }
            }

            ViewBag.Jobs = _context.Jobs
                .Select(j => new SelectListItem
                {
                    Value = j.JobId.ToString(),
                    Text = j.Title
                }).ToList();

            return View(model);
        }



        [Authorize(Roles = "HR,Admin")]

        //Delete : Applicant 
        public async Task<IActionResult> Delete(int id)
        {
            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant != null)
            {
                _context.Applicants.Remove(applicant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Applicant Deleted Succeessfully!";

            }
            return RedirectToAction(nameof(Index));

        }
        // GET: Details
        [Authorize(Roles = "HR,Admin")]

        public async Task<IActionResult> Details(int id)
        {
            var applicant = await _context.Applicants
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicantId == id);

            if (applicant == null)
            {
                return NotFound();
            }

            return View(applicant);
        }

        [Authorize(Roles = "HR,Admin")]

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, ApplicantStatus status)
        {
            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return NotFound();
            }

            applicant.Status = status;
            _context.Update(applicant);
            await _context.SaveChangesAsync();

            string subject = "Job Application Status Update";
            string message = $@"
              Dear {applicant.FirstName},

              Thank you for your interest in our company. We will get back to you soon regarding your application status.

              Best regards,  
              Hiring Team ";
             

            if (status == ApplicantStatus.Hired)
            {
                subject = "Congratulations! You’ve Been Hired!";
                message = $@"
                 Dear {applicant.FirstName},

                 We are pleased to inform you that you have been Hired for the position you applied for. 
                 Congratulations on your achievement! Our HR team will contact you shortly with further details 
                 regarding your joining process.

                 If you have any questions, feel free to reach out.

                 Best regards,  
                 Hiring Team ";
            }
            else if (status == ApplicantStatus.Rejected)
            {
                subject = "Job Application Status - Update";
                message = $@"
                 Dear {applicant.FirstName},

                 Thank you for applying to our company. After careful consideration, we regret to inform you 
                 that we have decided to move forward with another candidate at this time.

                 We truly appreciate your interest in the role, and we encourage you to apply for future opportunities 
                 that match your skills and experience.

                 Wishing you all the best in your job search!
                 
                 Best regards,  
                 Hiring Team ";
            }

            await _emailService.SendEmailAsync(applicant.Email, subject, message);


            TempData["SuccessMessage"] = $"Applicant status updated to {status} and email sent!";
            return RedirectToAction(nameof(Index));
        }




        [Authorize(Roles = "Applicant,HR,Admin")]

        public IActionResult Download(int id)
        {
            var applicant = _context.Applicants.FirstOrDefault(a => a.ApplicantId == id);
            if (applicant == null || string.IsNullOrEmpty(applicant.ResumePath))
            {
                return NotFound("Applicant or Resume file not found.");
            }

            string filePath = Path.Combine(_hostingEnvironment.WebRootPath + applicant.ResumePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            string downloadFileName = $"{applicant.FirstName}_{applicant.LastName}_cv.pdf";

            return File(fileBytes, "application/octet-stream", downloadFileName);
        }


        [Authorize(Roles = "Applicant,Admin")]
        public IActionResult ValidateApplicantId()
        {
            return View();
        }

        [Authorize(Roles = "Applicant,Admin")]
        [HttpPost]
        public async Task<IActionResult> ValidateApplicantIdPost(int ApplicantId)
        {
            if (ApplicantId <= 0)
            {
                TempData["Error"] = "Invalid Applicant ID.";
                return RedirectToAction(nameof(ValidateApplicantId));
            }

            var applicant = await _context.Applicants.FindAsync(ApplicantId);

            if (applicant == null)
            {
                TempData["Error"] = "Applicant not found.";
                return RedirectToAction(nameof(ValidateApplicantId));
            }

            return RedirectToAction(nameof(Edit), new { id = applicant.ApplicantId });
        }

        private int GenerateApplicantId()
        {
            Random random = new Random();
            return random.Next(100000, 999999); // 6 Digit Random Number
        }


    }
}
