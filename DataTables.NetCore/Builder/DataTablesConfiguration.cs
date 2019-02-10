using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTables.NetCore.Builder
{
    public class DataTablesConfiguration : ICloneable
    {
        public bool ServerSide { get; set; } = true;
        public string Ajax { get; set; }
        public string Method { get; set; }
        public string Dom { get; set; }

        public IList<DataTablesConfigurationColumn> Columns { get; protected set; } = new List<DataTablesConfigurationColumn>();

        public object Clone()
        {
            var copy = (DataTablesConfiguration)MemberwiseClone();

            copy.Columns = Columns.Select(c => (DataTablesConfigurationColumn)c.Clone()).ToList();

            return copy;
        }

        public class DataTablesConfigurationColumn : ICloneable
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }
    }
}
