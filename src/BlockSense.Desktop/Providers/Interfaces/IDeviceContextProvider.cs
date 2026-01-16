namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IDeviceContextProvider
    {
        string DeviceIdentifier
        {
            get; 
        }

        string DeviceOs
        {
            get;
        }

        string HardwareFingerprint
        {
            get;
        }

        string NetworkFingerprint
        {
            get;
        }
    }
}
