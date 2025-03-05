using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hiring_Application.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("SendGrid API key is missing in configuration.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("nakshtrasulakhe2613@gmail.com", "Hiring Team");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError($"Failed to send email to {toEmail}. Error: {errorBody}");
            }
        }

        public async Task SendApplicationCreatedEmailAsync(string toEmail, string applicantName, int applicantId)
        {
            string subject = "Application Submitted Successfully";
            string message = $"Dear {applicantName},\n\n" +
                             $"Your application has been successfully submitted.\n" +
                             $"Your Applicant ID is: {applicantId}.\n\n" +
                             "Please use this Applicant ID to edit your application.\n\n" +
                             "Thank you for applying!\n\nBest Regards,\nHiring Team";

            await SendEmailAsync(toEmail, subject, message);
        }
    }
}
