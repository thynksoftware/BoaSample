
using System;

namespace Boa.Sample.Services
{
    public interface IEmailHelperService
    {
        string AttachGuidToBody(Guid? guid, string htmlBody);
        
        string GetEmailGuidKey();
    }
}