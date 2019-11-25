using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class EmailSender: IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }
}
