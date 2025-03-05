using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Hiring_Application.Models;
using Hiring_Application.ViewModels;
using Hiring_Application.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace Hiring_Application.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;


    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
      _context = context;
        _userManager = userManager;

    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "HR,Admin")]
    public IActionResult HrDashboard()
    {
        var model = new HrDashboardViewModel
        {
            TotalApplicants = _context.Applicants.Count(),
            PendingApplicants = _context.Applicants.Count(a => a.Status == ApplicantStatus.Pending),
            RejectedApplicants = _context.Applicants.Count(a => a.Status == ApplicantStatus.Rejected),
            HiredApplicants = _context.Applicants.Count(a => a.Status == ApplicantStatus.Hired),
            InterviewScheduled = _context.Applicants.Count(a => a.Status == ApplicantStatus.InterviewScheduled),
            TotalJobsPosted = _context.Jobs.Count(),

            JobOpenings = _context.Jobs
         .Select(j => new JobOpeningViewModel
         {
             JobTitle = j.Title,
             Openings = j.Slots 
         })
         .ToList()
        };

        return View(model);

    }
    [Authorize(Roles = "Applicant,Admin")]
    public IActionResult ApplicantDashboard()
    {
        var model = new ApplicantDashboardViewModel
        {
            AvailableJobs = _context.Jobs.Where(j => j.Slots > 0).ToList()
        };
        return View(model);
    }

    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
