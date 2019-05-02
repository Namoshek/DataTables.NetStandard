using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.NetStandard.Extensions;
using Newtonsoft.Json;

namespace DataTables.NetStandard
{
    public class DataTablesConfiguration : ICloneable
    {
        public bool ServerSide { get; set; } = true;
        public AjaxConfiguration Ajax { get; set; } = new AjaxConfiguration();
        public IList<IList<string>> Order { get; set; } = new List<IList<string>>();

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

            copy.Order = Order.Select(o => (IList<string>)o.Select(c => (string)c.Clone()).ToList()).ToList();
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

        /// <summary>
        /// Part of the DataTables configuration for server-side processing.
        /// </summary>
        public class AjaxConfiguration
        {
            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Method { get; set; }
        }
    }
}
