using System.Threading.Tasks;
using Boa.Sample.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Boa.Sample.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(
            SmtpSettings smtpSettings,
            ImapSettings imapSettings,
            EmailCredentials credentials,
            BaseEmailMessage<string> message);
    }
}