using BlockSense.Backend.Exceptions.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlockSense.Backend.Models.DeviceContext
{
    public class DeviceContextModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
                throw new ArgumentNullException(nameof(bindingContext));

            try
            {
                var deviceContext = DeviceContext.FromHttpContext(bindingContext.HttpContext);
                bindingContext.Result = ModelBindingResult.Success(deviceContext);
            }
            catch (InvalidClientContextException ex)
            {
                bindingContext.ModelState.AddModelError("DeviceContext", ex.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
