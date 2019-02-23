using Newtonsoft.Json.Converters;

namespace DataTables.NetStandard.Enhanced.Sample.Util
{
    public class LocalDateTimeConverter : IsoDateTimeConverter
    {
        public LocalDateTimeConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
