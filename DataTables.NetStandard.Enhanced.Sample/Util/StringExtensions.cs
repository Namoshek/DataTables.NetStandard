namespace DataTables.NetStandard.Enhanced.Sample.Util
{
    public static class StringExtensions
    {
        public static int ParseAsIntOrDefault(this string s, int defaultValue)
        {
            if (int.TryParse(s, out int result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}
