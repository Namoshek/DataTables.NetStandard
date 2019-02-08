using System;

namespace DataTables.NetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DataTableColumn : Attribute
    {
        public string DisplayName { get; set; }
        public string Data { get; set; }
        public string QueryData { get; set; }
        public bool IsSortable { get; set; }
        public bool IsSearchable { get; set; }

        public DataTableColumn(string data, string displayName = null, string queryData = null, bool isSortable = false, bool isSearchable = false)
        {
            Data = data;
            DisplayName = displayName ?? data.FirstCharToUpper();
            QueryData = queryData ?? data.FirstCharToUpper();
            IsSortable = isSortable;
            IsSearchable = isSearchable;
        }
    }
}
