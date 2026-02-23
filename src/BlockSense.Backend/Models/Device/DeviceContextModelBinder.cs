using BlockSense.Backend.Exceptions.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BlockSense.Backend.Models.Device
{
    public class DeviceContextModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var deviceContext = DeviceContext.FromHttpContext(bindingContext.HttpContext);
            bindingContext.Result = ModelBindingResult.Success(deviceContext);

            Validator.ValidateObject(deviceContext, new ValidationContext(deviceContext), validateAllProperties: true);



            return Task.CompletedTask;
        }
    }
}
