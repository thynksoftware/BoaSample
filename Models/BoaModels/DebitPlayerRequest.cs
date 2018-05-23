
namespace Boa.Sample.Models
{
    public class DebitPlayerRequest : BaseBrandApiRequest
    {
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransactionId { get; set; }
    }
}