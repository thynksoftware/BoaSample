using System.Collections.Generic;

namespace Boa.Sample.Models
{
    public class JackpotEventSettledRequest
    {
        public string EventName { get; set; }
        public List<JackpotEventSettledRequestItem> Items { get; set; } = new List<JackpotEventSettledRequestItem>();
    }
}