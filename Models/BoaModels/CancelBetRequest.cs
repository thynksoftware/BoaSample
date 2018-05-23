
using System;

namespace Boa.Sample.Models
{
    public class CancelBetRequest : BaseBrandApiRequest
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public Guid? TransactionGuid { get; set; }
    }
}