namespace Boa.Sample.Models
{
    public class EmailAttachment
    {
        
        public string ContentId { get; set; }
        
        public string ContentType { get; set; }
        
        public string AttachmentName { get; set; }

        public byte[] Content { get; set; }
    }
}