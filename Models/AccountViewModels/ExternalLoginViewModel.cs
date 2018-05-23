using System.ComponentModel.DataAnnotations;

namespace Boa.Sample.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
