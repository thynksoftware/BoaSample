using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Boa.Sample.Data;

namespace Boa.Sample
{

    public class EmailValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (BoaIntegrationDbContext) validationContext.GetService(typeof(BoaIntegrationDbContext));
            if (value != null)
            {
                var valueAsString = value.ToString();
                var emailExists = db != null && db.Users.Any(x => x.Email == valueAsString);
                if (emailExists)
                {
                    var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                    return new ValidationResult(errorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }

    public sealed class EighteenAndOverDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // your validation logic
            if (Convert.ToDateTime(value) <= DateTime.Today.AddYears(-18))
            {
                return ValidationResult.Success;
            }
            else
            {
                var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(errorMessage);
            }
        }
    }
}