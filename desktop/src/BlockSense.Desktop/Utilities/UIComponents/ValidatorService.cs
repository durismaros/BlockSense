using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BlockSense.Desktop.Utilities.UIComponents
{
    public static class ValidatorService
    {
        public static bool TryValidate<T>(T model, out string errorMessage)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            errorMessage = isValid ? string.Empty : results.First().ErrorMessage;
            return isValid;
        }
    }
}
