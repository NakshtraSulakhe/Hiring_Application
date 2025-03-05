using Hiring_Application.Models;

namespace Hiring_Application.ViewModels
{
    public class HrDashboardViewModel
    {
        public int TotalApplicants { get; set; }
        public int PendingApplicants { get; set; }
        public int RejectedApplicants { get; set; }
        public int HiredApplicants { get; set; }
        public int InterviewScheduled { get; set; }
        public int TotalJobsPosted { get; set; }
        public List<JobOpeningViewModel> JobOpenings { get; set; }
    }

    public class JobOpeningViewModel
    {
        public string JobTitle { get; set; }
        public int Openings { get; set; }
    }

    public class ApplicantDashboardViewModel
    {
        public List<Job> AvailableJobs { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalApplicants { get; set; }
        public int TotalJobs { get; set; }
        public int TotalHired { get; set; }
    }


}
