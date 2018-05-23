namespace Boa.Sample.Models
{
    public class EmailCredentials
    {
        
        public EmailCredentials(){}

        public EmailCredentials(string email, string password, string accessEndPoint = null)
        {
            Email = email;
            Password = password;
            AccessEndPoint = accessEndPoint;
        }

        public EmailCredentials(EmailCredentials credentials)
        {
            Email = credentials.Email;
            Password = credentials.Password;
            AccessEndPoint = credentials.AccessEndPoint;
        }
        
        public string Email { get; set; }
        
        public string Password { get; set; }

        public string AccessEndPoint { get; set; }
    }
}