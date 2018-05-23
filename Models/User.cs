using System;
using Microsoft.AspNetCore.Identity;

namespace Boa.Sample.Models
{
    // Add profile data for application users by adding properties to the User class
    public class BaseViewModel
    {
        public string LanguageCode { get; set; } = "en";
    }

    public class User : IdentityUser
    {
        public string Token { get; set; } = Guid.NewGuid().ToString();
        public decimal Amount { get; set; } = 300;
        public decimal? PlayerLimit { get; set; } = 200;
        public string CurrencyCode { get; set; } = "EUR";
        public string LanguageCode { get; set; } = "en";
        public bool IsActive { get; set; } = true;
        public string Name { get; set; }
        public string Surname { get; set; }

    }
}
