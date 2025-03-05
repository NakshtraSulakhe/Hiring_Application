using Hiring_Application.Models;
using System.ComponentModel.DataAnnotations;

namespace Hiring_Application.Models
{


    public class Job
    {
        public int JobId { get; set; }

        [Required(ErrorMessage = "Job Title is required")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime DatePosted { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Slots are required")]
        [Range(1, int.MaxValue, ErrorMessage = "Minimum 1 slot required")]
        public int Slots { get; set; }

        public ICollection<Applicant>? Applicants { get; set; }
    }
}