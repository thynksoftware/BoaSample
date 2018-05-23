
using System;
using System.Collections.Generic;

namespace Boa.Sample.Models
{
    public class BaseEmailMessage<T>
    {
        public string ToEmail { get; set; }

        public string Subject { get; set; }

        public List<EmailAttachment> Attachments { get; set; }

        public T Body { get; set; }

        public string[] BccList { get; set; }

        public string[] CcList { get; set; }

        public Guid? TrackingGuid { get; set; }

        public List<EmailHeader> CustomHeaders { get; set; }

        public string SentFolder { get; set; }
    }
}