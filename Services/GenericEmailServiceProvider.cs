
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Boa.Sample.Extentions;
using Boa.Sample.Models;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;

namespace Boa.Sample.Services
{
    internal class GenericEmailServiceProvider : IEmailServiceProvider
    {
        private readonly IViewRenderService _viewRenderService;
        private readonly IEmailHelperService _emailHelperService;
        private readonly ICacheRepository _cacheRepository;

        public GenericEmailServiceProvider(IEmailHelperService emailHelperService, IViewRenderService viewRenderService, ICacheRepository cacheRepository)
        {
            _viewRenderService = viewRenderService;
            _emailHelperService = emailHelperService;
            _cacheRepository = cacheRepository;
        }

        public virtual async Task<bool> SendEmailAsync(
            SmtpSettings smtpSettings,
            ImapSettings imapSettings,
            EmailCredentials credentials,
            BaseEmailMessage<string> message)
        {
            var streams = new List<Stream>();
            try
            {
                var emailMessage = GetEmailMessage(credentials.Email, message);
                if (emailMessage == null)
                {
                    return false;
                }
                message.Body = _emailHelperService.AttachGuidToBody(message.TrackingGuid, message.Body);

                var bodyBuilder = new BodyBuilder {HtmlBody = message.Body};

                using (var client = new SmtpClient())
                {
                    if (message.Attachments != null)
                    {
                        PrepareAttachments(emailMessage, streams, message.Attachments, bodyBuilder);
                    }
                    else
                    {
                        emailMessage.Body = bodyBuilder.ToMessageBody();
                    }
                    await client.ConnectAsync(smtpSettings.SmtpHost, smtpSettings.SmtpPort);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(credentials.Email, credentials.Password);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                    if (!string.IsNullOrWhiteSpace(message.SentFolder))
                    {
                        AppendToFolder(imapSettings, credentials, emailMessage, message.SentFolder);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            finally
            {
                streams.ForEach(x => { x.Dispose(); });
            }
        }

        private static void AppendToFolder(ImapSettings settings, EmailCredentials credentials,
            MimeMessage emailMessage, string folder)
        {
            using (var imapClient = new ImapClient())
            {
                imapClient.Connect(settings.ImapHost, settings.ImapPort);
                imapClient.Authenticate(credentials.Email, credentials.Password);
                var personal = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);
                var sentFolder = personal.GetSubfolders()
                    .FirstOrDefault(x => x.Name.ToLowerInvariant() == folder.ToLowerInvariant());
                if (sentFolder != null)
                {
                    sentFolder.Append(emailMessage);
                    sentFolder.Close();
                }

                imapClient.Disconnect(true);
            }
        }

        private static MimeMessage GetEmailMessage(string fromEmail, BaseEmailMessage<string> message)
        {
            var emailMessage = new MimeMessage();
            if (string.IsNullOrWhiteSpace(message.ToEmail) || string.IsNullOrWhiteSpace(fromEmail)) return null;
            emailMessage.From.Add(new MailboxAddress(fromEmail));
            emailMessage.To.Add(new MailboxAddress(message.ToEmail));
            if (!message.BccList.IsEmpty())
            {
                message.BccList.ForEach(bcc => { emailMessage.Bcc.Add(new MailboxAddress(bcc)); });
            }
            if (!message.CcList.IsEmpty())
            {
                message.CcList.ForEach(cc => { emailMessage.Cc.Add(new MailboxAddress(cc)); });
            }
            message.CcList.ForEach(cc => { emailMessage.Cc.Add(new MailboxAddress(cc)); });
            if (!string.IsNullOrWhiteSpace(message.Subject))
            {
                emailMessage.Subject = message.Subject;
            }
            if (!message.CustomHeaders.IsEmpty())
            {
                message.CustomHeaders.ForEach(keyValuePair =>
                {
                    emailMessage.Headers.Add(keyValuePair.Name, keyValuePair.Value);
                });
            }
            return emailMessage;
        }

        private static void PrepareAttachments(MimeMessage emailMessage, List<Stream> streams,
            List<EmailAttachment> attachments, BodyBuilder bodyBuilder)
        {
            var multipart = new Multipart("mixed") {bodyBuilder.ToMessageBody()};
            try
            {
                foreach (var attachment in attachments)
                {
                    Stream stream = new MemoryStream(attachment.Content);
                    var contentObject = new MimeKit.MimeContent(stream);
                    streams.Add(stream);
                    var contentDisposition = new ContentDisposition(ContentDisposition.Attachment);
                    var fileType = stream.GetFileTypeFromStream();
                    attachment.AttachmentName = attachment.AttachmentName.EndsWith(fileType.Extension)
                        ? attachment.AttachmentName
                        : $"{attachment.AttachmentName}.{fileType.Extension}";
                    var attachmentPart = new MimePart(MimeTypes.GetMimeType(attachment.AttachmentName))
                    {
                        Content = contentObject,
                        ContentDisposition = contentDisposition,
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachment.AttachmentName
                    };
                    multipart.Add(attachmentPart);
                }
                emailMessage.Body = multipart;
            }
            catch
            {
                // ignored
            }
        }
    }
}