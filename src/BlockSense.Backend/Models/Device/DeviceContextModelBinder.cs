using BlockSense.Backend.Exceptions.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BlockSense.Backend.Models.Device
{
    /// <summary>
    /// Binds a <see cref="DeviceContext"/> from the current HTTP request by extracting device-specific headers.
    /// </summary>
    public sealed class DeviceContextModelBinder : IModelBinder
    {
        /// <summary>
        /// Attempts to bind a <see cref="DeviceContext"/> from the current <see cref="ModelBindingContext"/>.
        /// </summary>
        /// <param name="bindingContext">The context used to perform model binding.</param>
        /// <returns>A completed <see cref="Task"/> representing the binding operation.</returns>
        /// <exception cref="InvalidClientContextException">Thrown if any required device header is missing or the context is invalid.</exception>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var deviceContext = DeviceContext.FromHttpContext(bindingContext.HttpContext);

            Validator.ValidateObject(
                deviceContext,
                new ValidationContext(deviceContext),
                validateAllProperties: true);

            bindingContext.Result = ModelBindingResult.Success(deviceContext);

            return Task.CompletedTask;
        }
    }
}