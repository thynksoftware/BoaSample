namespace Boa.Sample.Models
{

    public class AuthenticatePlayerResponse : BaseBrandApiResponse
    {
        public string PlayerId { get; set; }
        public string NickName { get; set; }
        public string CurrencyCode { get; set; }
        public string LanguageCode { get; set; }
        public decimal? PlayerLimit { get; set; }
        public string Email { get; set; }
    }
}