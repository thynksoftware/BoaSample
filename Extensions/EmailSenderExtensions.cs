using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Boa.Sample.Models;
using Boa.Sample.Services;
using Microsoft.Extensions.Options;

namespace Boa.Sample.Extensions
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailService emailSender,EmailSettings emailSettings, string email, string link)
        {
            var message = new BaseEmailMessage<string>()
            {
                Body =  $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>",
                SentFolder = emailSettings.SentFolder,
                Subject =  "Confirm your email",
                ToEmail = email,
                CcList = new string[]{},
                BccList = new string[]{}
            };
                
            return emailSender.SendEmailAsync(emailSettings.SmtpSettings,emailSettings.ImapSettings,emailSettings.EmailCredentials, message);
        }
    }
}
