using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BlockSense.Desktop.Utilities.Validation
{
    /// <summary>
    /// Provides helper methods to validate objects using <see cref="System.ComponentModel.DataAnnotations"/> attributes, simplifying model validation in the BlockSense desktop application.
    /// </summary>
    public static class DataAnnotationsValidator
    {
        /// <summary>
        /// Validates the specified model against its <see cref="ValidationAttribute"/> annotations.
        /// </summary>
        /// <typeparam name="T">The type of the model to validate.</typeparam>
        /// <param name="model">The object instance to validate.</param>
        /// <param name="errorMessage">Returns the first validation error message if validation fails; otherwise, an empty string.</param>
        /// <returns><c>true</c> if the model is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
        public static bool TryValidate<T>(T model, out string errorMessage)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            // Create a validation context and collect validation results
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                instance: model,
                validationContext: context,
                validationResults: results,
                validateAllProperties: true);

            // Return first error message or empty string if valid
            errorMessage = isValid ? string.Empty : results.First().ErrorMessage ?? string.Empty;

            return isValid;
        }
    }
}
