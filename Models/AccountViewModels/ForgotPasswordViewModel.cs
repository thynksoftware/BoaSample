using System.ComponentModel.DataAnnotations;

namespace Boa.Sample.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Error { get; set; }
    }
}
