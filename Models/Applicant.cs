using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiring_Application.Models
{


    public class Applicant
    {
        public int ApplicantId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Entey Your  City")]
        [StringLength(50)]
        public string City { get; set; }
        public string? ResumePath { get; set; } 

        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        [Required]
        public ApplicantStatus Status { get; set; } = ApplicantStatus.Pending;

        [ForeignKey("Job")]
        public int JobId { get; set; }
        public Job? Job { get; set; }


    }
    public enum ApplicantStatus
    {
        Pending,
        InterviewScheduled,
        Hired,
        Rejected
    }
}
