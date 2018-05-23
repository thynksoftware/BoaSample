namespace Boa.Sample.Models
{
    public class JackpotEventSettledRequestItem
    {
        public decimal Money { get; set; }
        public decimal Points { get; set; }
        public string PlayerId { get; set; }
        public long BetId { get; set; }
    }
}