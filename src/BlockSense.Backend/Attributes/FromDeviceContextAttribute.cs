using BlockSense.Backend.Models.Device;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlockSense.Backend.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromDeviceContextAttribute : ModelBinderAttribute
    {
        public FromDeviceContextAttribute()
        {
            BinderType = typeof(DeviceContextModelBinder);
            BindingSource = BindingSource.Custom;
        }
    }
}
