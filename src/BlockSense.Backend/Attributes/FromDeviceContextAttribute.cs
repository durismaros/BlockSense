using BlockSense.Backend.Models.Device;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlockSense.Backend.Attributes
{
    /// <summary>
    /// Marks an action parameter to be bound from the current HTTP request's device context headers
    /// using <see cref="DeviceContextModelBinder"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromDeviceContextAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FromDeviceContextAttribute"/>
        /// and configures it to use <see cref="DeviceContextModelBinder"/>.
        /// </summary>
        public FromDeviceContextAttribute()
        {
            BinderType = typeof(DeviceContextModelBinder);
            BindingSource = BindingSource.Custom;
        }
    }
}