using System;
using DataTables.NetCore.Extensions;

namespace DataTables.NetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DTColumn : Attribute
    {
        public int Index { get; set; }
        public string Data { get; set; }
        public string QueryName { get; set; }
        public string DisplayName { get; set; }
        public bool IsSortable { get; set; }
        public bool IsSearchable { get; set; }

        public DTColumn(
            string data, 
            string displayName = null, 
            string queryData = null, 
            bool isSortable = false, 
            bool isSearchable = false,
            int index = -1
        )
        {
            Data = data;
            DisplayName = displayName ?? data.FirstCharToUpper();
            QueryName = queryData ?? data.FirstCharToUpper();
            IsSortable = isSortable;
            IsSearchable = isSearchable;
            Index = index;
        }
    }
}
