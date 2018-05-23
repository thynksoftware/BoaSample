
namespace Boa.Sample.Models
{
    public class CreditPlayerRequest : BaseBrandApiRequest
    {
        public string Reason { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public int? BetId { get; set; }
        public string PerviousTransactionId { get; set; }
        public string TransactionId { get; set; }
    }
}