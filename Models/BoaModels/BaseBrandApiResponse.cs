
namespace Boa.Sample.Models
{
    public abstract class BaseBrandApiResponse
    {
        public BrandApiErrorCode Code { get; set; }
        public string Status { get; set; }
    }
}