using System.Threading.Tasks;
using Boa.Sample.Models;

namespace Boa.Sample.Services
{
    internal interface IEmailServiceProvider
    {
        Task<bool> SendEmailAsync(
            SmtpSettings smtpSettings,
            ImapSettings imapSettings,
            EmailCredentials credentials,
            BaseEmailMessage<string> message);

    }
}