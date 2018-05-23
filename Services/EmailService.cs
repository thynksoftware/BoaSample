using System.Threading.Tasks;
using Boa.Sample.Models;

namespace Boa.Sample.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailHelperService _emailHelperService;
        private readonly IViewRenderService _viewRenderService;
        private readonly ICacheRepository _cacheRepository;

        private const string Office365ImapHost = "outlook.office365.com";
        private const int Office365ImapPort = 993;

        private const string Office365SmtpHost = "smtp.office365.com";
        private const int Office365SmtpPort = 587;

        public EmailService(IEmailHelperService emailHelperService, IViewRenderService viewRenderService, ICacheRepository cacheRepository)
        {
            _emailHelperService = emailHelperService;
            _viewRenderService = viewRenderService;
            _cacheRepository = cacheRepository;
        }


        public virtual async Task<bool> SendEmailAsync(
            SmtpSettings smtpSettings,
            ImapSettings imapSettings,
            EmailCredentials credentials,
            BaseEmailMessage<string> message)
            =>
                await GetEmailProvider(imapSettings, smtpSettings).SendEmailAsync(smtpSettings,
                    imapSettings,
                    credentials,
                    message);



        private IEmailServiceProvider GetEmailProvider(ImapSettings imapSettings, SmtpSettings smtpSettings)
        {
            var imapProvider = GetImapProvider(imapSettings);
            var smtpProvider = GetSmtpProvider(smtpSettings);
            if (smtpProvider == EmailProviderEnum.Office365 && imapProvider == EmailProviderEnum.Office365)
            {
                return new Office365EmailServiceProvider(_emailHelperService, _viewRenderService, _cacheRepository);
            }
            return new GenericEmailServiceProvider(_emailHelperService, _viewRenderService, _cacheRepository);
        }

        public EmailProviderEnum GetImapProvider(ImapSettings settings)
        {
            if (settings.ImapHost == Office365ImapHost && settings.ImapPort == Office365ImapPort)
            {
                return EmailProviderEnum.Office365;
            }
            return EmailProviderEnum.Generic;
        }

        public EmailProviderEnum GetSmtpProvider(SmtpSettings smtpSettings)
        {
            if (smtpSettings.SmtpHost == Office365SmtpHost && smtpSettings.SmtpPort == Office365SmtpPort)
            {
                return EmailProviderEnum.Office365;
            }
            return EmailProviderEnum.Generic;
        }


    }
}