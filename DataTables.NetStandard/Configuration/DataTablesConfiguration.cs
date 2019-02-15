using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.NetStandard.Extensions;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Configuration
{
    public class DataTablesConfiguration : ICloneable
    {
        public bool ServerSide { get; set; } = true;
        public string Ajax { get; set; }
        public string Method { get; set; }

        /// <summary>
        /// A dictionary of additional options that will be passed to DataTable instances.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, dynamic> AdditionalOptions { get; set; } = new Dictionary<string, dynamic>();

        /// <summary>
        /// A list of columns required for DataTable instances. Not relevant for global configuration.
        /// </summary>
        public IList<DataTablesConfigurationColumn> Columns { get; protected set; } = new List<DataTablesConfigurationColumn>();


        public object Clone()
        {
            var copy = (DataTablesConfiguration)MemberwiseClone();

            copy.AdditionalOptions = AdditionalOptions.DeepClone();
            copy.Columns = Columns.Select(c => (DataTablesConfigurationColumn)c.Clone()).ToList();

            return copy;
        }


        /// <summary>
        /// A utility class representing a DataTable column. Gets serialized as part of the DataTable configuration script.
        /// </summary>
        /// <seealso cref="ICloneable" />
        public class DataTablesConfigurationColumn : ICloneable
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }

            /// <summary>
            /// A dictionary of additional options that will be passed to DataTable instances.
            /// </summary>
            [JsonExtensionData]
            public IDictionary<string, dynamic> AdditionalOptions { get; set; } = new Dictionary<string, dynamic>();


            public object Clone()
            {
                return MemberwiseClone();
            }
        }
    }
}
