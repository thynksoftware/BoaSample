namespace Boa.Sample.Models
{
    public class EmailSettings
    {

        public const string ConfigurationKey = "EmailSettings";

        public SmtpSettings SmtpSettings { get; set; }
        
        public ImapSettings ImapSettings { get; set; }

        public EmailCredentials EmailCredentials { get; set; }

        public string CcList { get; set; }
        
        public string BccList { get; set; }
        
        public string SentFolder { get; set; }

        public EmailSettings Clone()
        {
            var clone = new EmailSettings()
            {
                SmtpSettings = new SmtpSettings(SmtpSettings),
                ImapSettings = new ImapSettings(ImapSettings),
                BccList = BccList,
                CcList =  CcList,
                EmailCredentials = new EmailCredentials(EmailCredentials),
                SentFolder = SentFolder
            };
            return clone;
        }
    }
}