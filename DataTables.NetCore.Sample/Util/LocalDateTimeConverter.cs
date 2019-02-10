using Newtonsoft.Json.Converters;

namespace DataTables.NetCore.Sample.Util
{
    public class LocalDateTimeConverter : IsoDateTimeConverter
    {
        public LocalDateTimeConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
