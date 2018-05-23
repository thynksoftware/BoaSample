namespace Boa.Sample.Models
{
    public class ImapSettings
    {
        public ImapSettings(){}

        public ImapSettings(string host, int port)
        {
            ImapHost = host;
            ImapPort = port;
        }

        public ImapSettings(ImapSettings settings)
        {
            ImapHost = settings.ImapHost;
            ImapPort = settings.ImapPort;
        }
        
        public string ImapHost { get; set; }
        
        public int ImapPort { get; set; }
    }
}