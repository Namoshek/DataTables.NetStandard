using Newtonsoft.Json.Converters;

namespace DataTables.NetStandard.Sample.Util
{
    public class LocalDateTimeConverter : IsoDateTimeConverter
    {
        public LocalDateTimeConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
