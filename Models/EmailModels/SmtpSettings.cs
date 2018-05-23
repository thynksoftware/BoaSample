namespace Boa.Sample.Models
{
    public class SmtpSettings
    {
        
        public SmtpSettings(){}

        public SmtpSettings(string host, int port)
        {
            SmtpHost = host;
            SmtpPort = port;
        }
        
        public SmtpSettings(SmtpSettings settings)
        {
            SmtpHost = settings.SmtpHost;
            SmtpPort = settings.SmtpPort;
        }
        
        public string SmtpHost { get; set; }
        
        public int SmtpPort { get; set; }
    }
}