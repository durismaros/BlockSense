using MaxMind.GeoIP2;

namespace BlockSenseAPI.Services
{
    public class GeoLookupService
    {
        private readonly string _filePath;
        private DatabaseReader _reader;

        public GeoLookupService()
        {
            _filePath = @"C:\Users\d3str\Desktop\School\BlockSense\BlockSenseAPI\BlockSenseAPI\Properties\GeoLite2-Country.mmdb"; // Downloaded from MaxMind
            _reader = new DatabaseReader(_filePath);
        }
        public string? GetCountry(string ipAddress)
        {
             // Initialize the reader
            try
            {
                var location = _reader.Country(ipAddress);
                return location.Country.IsoCode;
            }
            catch
            {
                return null; // Unknown/private IP
            }
            finally
            {
                _reader.Dispose();
            }
        }
    }
}
