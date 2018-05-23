
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boa.Sample.Extentions;
using Boa.Sample.Models;
using Microsoft.Exchange.WebServices.Autodiscover;
using Microsoft.Exchange.WebServices.Data;

namespace Boa.Sample.Services
{
    internal class Office365EmailServiceProvider : IEmailServiceProvider
    {
        private readonly IEmailHelperService _emailHelperService;
        private readonly IViewRenderService _viewRenderService;
        private readonly ICacheRepository _cacheRepository;

        public Office365EmailServiceProvider(IEmailHelperService emailHelperService,
            IViewRenderService viewRenderService, ICacheRepository cacheRepository)
        {
            _emailHelperService = emailHelperService;
            _viewRenderService = viewRenderService;
            _cacheRepository = cacheRepository;
        }

        public async Task<bool> SendEmailAsync(
            SmtpSettings smtpSettings,
            ImapSettings imapSettings,
            EmailCredentials credentials,
            BaseEmailMessage<string> baseEmailMessage)
        {
            var service = GetExchangeService(credentials);
            var message = PrepareMessage(service, baseEmailMessage);
            PrepareAttachments(message, baseEmailMessage.Attachments);
            await message.SendAndSaveCopy(WellKnownFolderName.SentItems);

            return true;
        }

        protected virtual void PrepareAttachments(EmailMessage emailMessage, List<EmailAttachment> attachments)
        {
            try
            {
                if (attachments.IsEmpty()) return;
                foreach (var attachment in attachments)
                {
                    var fileType = FileTypeResolver.GetFileTypeForHeader(attachment.Content);
                    attachment.AttachmentName = attachment.AttachmentName.EndsWith(fileType.Extension)
                        ? attachment.AttachmentName
                        : $"{attachment.AttachmentName}.{fileType.Extension}";
                    emailMessage.Attachments.AddFileAttachment(attachment.AttachmentName, attachment.Content);
                }
            }
            catch
            {
                // ignored
            }
        }

        private EmailMessage PrepareMessage(ExchangeService service, BaseEmailMessage<string> baseEmailMessage)
        {
            baseEmailMessage.Body = _emailHelperService.AttachGuidToBody(baseEmailMessage.TrackingGuid, baseEmailMessage.Body);

            var message = new EmailMessage(service)
            {
                Body = baseEmailMessage.Body
            };

            if (!string.IsNullOrWhiteSpace(baseEmailMessage.Subject))
            {
                message.Subject = baseEmailMessage.Subject;
            }

            if (!baseEmailMessage.BccList.IsEmpty())
            {
                baseEmailMessage.BccList.ForEach(email =>
                {
                    message.BccRecipients.Add(email);
                });
            }

            if (!baseEmailMessage.CcList.IsEmpty())
            {
                baseEmailMessage.CcList.ForEach(email =>
                {
                    message.CcRecipients.Add(email);
                });
            }

            message.ToRecipients.Add(baseEmailMessage.ToEmail);

            if (baseEmailMessage.CustomHeaders == null)
            {
                baseEmailMessage.CustomHeaders = new List<EmailHeader>();
            }
            baseEmailMessage.CustomHeaders.Add(GetGuidEmailHeader(baseEmailMessage.TrackingGuid.ToString().ToLower()));

            if (!baseEmailMessage.CustomHeaders.IsEmpty())
            {
                baseEmailMessage.CustomHeaders.Where(x => x.Guid.HasValue).ForEach(header =>
                {
                    // Get the GUID for the property set.
                    var guid = header.Guid;

                    // Create a definition for the extended property.
                    if (guid == null) return;
                    var extendedPropertyDefinition =
                        new ExtendedPropertyDefinition(guid.Value, header.Name, MapiPropertyType.String);

                    if (!string.IsNullOrWhiteSpace(header.Value))
                    {
                        message.SetExtendedProperty(extendedPropertyDefinition, header.Value);
                    }
                });
            }
            return message;
        }

        private ExchangeService GetExchangeService(EmailCredentials credentials)
        {
            var url = credentials.AccessEndPoint;
            var cacheKey = BuildCacheKey(credentials.Email);
            if (string.IsNullOrWhiteSpace(url))
            {
                url = _cacheRepository.Get<string>(cacheKey);   
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                var autodiscover = new AutodiscoverService(ExchangeVersion.Exchange2013_SP1)
                {
                    RedirectionUrlValidationCallback = x => true,
                    Credentials = new WebCredentials(credentials.Email, credentials.Password)
                };
                var userSettings = autodiscover.GetUsersSettings(new List<string>() { credentials.Email }, UserSettingName.ExternalEwsUrl);

                var successResponse = userSettings.First(x => x.ErrorCode == AutodiscoverErrorCode.NoError);
                successResponse.TryGetSettingValue(UserSettingName.ExternalEwsUrl, out url);
                _cacheRepository.Put(cacheKey, url, new CacheRepositoryOptions());
            }
            var service =
                new ExchangeService(ExchangeVersion.Exchange2013_SP1)
                {
                    Credentials = new WebCredentials(credentials.Email, credentials.Password),
                    Url = new Uri(url)
                };

            return service;
        }

        private string BuildCacheKey(string email)
        {
            return $"outlook_{email}";
        }

        private EmailHeader GetGuidEmailHeader(string value)
        {
            return new EmailHeader()
            {
                Guid = Guid.Parse("30db178f-a50a-4f62-b2eb-c6efe301178a"),
                Name = _emailHelperService.GetEmailGuidKey(),
                Value = value
            };
        }
    }
}