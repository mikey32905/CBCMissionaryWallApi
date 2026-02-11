using Microsoft.AspNetCore.Identity.UI.Services;

namespace CBCMissionaryWallApi.Services
{
    public class ConsoleEmailService : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"To: {email}");
            Console.WriteLine("Subject: {subject}");
            Console.WriteLine($"{htmlMessage}");

            return Task.CompletedTask;
        }
    }
}
