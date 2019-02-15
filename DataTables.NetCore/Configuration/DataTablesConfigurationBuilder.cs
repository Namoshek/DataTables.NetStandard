using System.Collections.Generic;
using DataTables.NetCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataTables.NetCore.Configuration
{
    public class DataTablesConfigurationBuilder
    {
        /// <summary>
        /// A global singleton-like default configuration for DataTables.
        /// </summary>
        public static DataTablesConfiguration DefaultConfiguration { get; } = new DataTablesConfiguration();

        /// <summary>
        /// Builds the global configuration script containing all global DataTables settings.
        /// </summary>
        public static string BuildGlobalConfigurationScript()
        {
            var output = JsonConvert.SerializeObject(DefaultConfiguration, GetSerializerSettings());

            return $"$.extend(true, $.fn.dataTable.defaults, {output});";
        }

        /// <summary>
        /// Builds the configuration script for a specific DataTable.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="columns">The columns.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        public static string BuildDataTableConfigurationScript<TEntity, TEntityViewModel>(IList<DataTablesColumn<TEntity, TEntityViewModel>> columns, 
            string tableName, string url, string method, IDictionary<string, dynamic> additionalOptions = null)
        {
            var configuration = (DataTablesConfiguration)DefaultConfiguration.Clone();
            configuration.Ajax = url;
            configuration.Method = method;

            foreach (var column in columns)
            {
                configuration.Columns.Add(new DataTablesConfiguration.DataTablesConfigurationColumn
                {
                    Data = column.PublicName,
                    Name = column.PublicName,
                    Title = column.DisplayName,
                    Searchable = column.IsSearchable,
                    Orderable = column.IsOrderable,
                    AdditionalOptions = column.AdditionalOptions.DeepClone()
                });
            }

            // We override the existing global DataTables configuration with the custom per-table config
            if (additionalOptions != null)
            {
                foreach (var option in additionalOptions)
                {
                    configuration.AdditionalOptions[option.Key] = option.Value;
                }
            }

            var config = JsonConvert.SerializeObject(configuration, GetSerializerSettings());

            return $"var dt_{tableName} = $('#{tableName}').DataTable({config});";
        }

        /// <summary>
        /// Gets the serializer settings used to serialize the DataTables configurations.
        /// </summary>
        protected static JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
