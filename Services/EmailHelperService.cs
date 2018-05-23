
using System;
using Microsoft.AspNetCore.Http;

namespace Boa.Sample.Services
{
    public class EmailHelperService : IEmailHelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string EmailGuidKey = "TrackingGuid";
        
        public EmailHelperService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public string AttachGuidToBody(Guid? guid, string htmlBody)
        {
            if (guid == null) return htmlBody;
            var server = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
            var tracker =
                $"<img src='{server}/api/v1/EmailTracking/SetEmailAsRead?guid={guid.ToString().ToLower()}' /> ";
            htmlBody = htmlBody + tracker;
            return htmlBody;
        }
        
        public string GetEmailGuidKey()
        {
            return EmailGuidKey;
        }
    }
}