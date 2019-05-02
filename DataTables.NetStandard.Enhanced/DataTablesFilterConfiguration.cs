using System;
using System.Collections.Generic;

namespace DataTables.NetStandard.Enhanced
{
    public class DataTablesFilterConfiguration
    {
        /// <summary>
        /// Additional filter options that are used for the filter with the given type.
        /// Used as filter options for the yadcf DataTables filter system.
        /// </summary>
        protected IDictionary<Type, IDictionary<string, dynamic>> _additionalColumnFilterOptions
            = new Dictionary<Type, IDictionary<string, dynamic>>();

        /// <summary>
        /// Defines whether select filters should display a default label like 'Select value'.
        /// </summary>
        public bool EnableDefaultSelectionLabel { get; set; } = true;

        /// <summary>
        /// Defines the default label displayed on select filters if <see cref="EnableDefaultSelectionLabel"/>
        /// is enabled. Can be used to localize the filters.
        /// </summary>
        public string DefaultSelectionLabelValue { get; set; } = "Select value";

        /// <summary>
        /// Defines the default placerholder displayed in text inputs. Can be used to localize
        /// the filter.
        /// </summary>
        public string DefaultTextInputPlaceholderValue { get; set; } = "Type to filter";

        /// <summary>
        /// Additional filter options used to initialize the yadcf DataTables filter system.
        /// </summary>
        public IDictionary<string, dynamic> AdditionalFilterOptions { get; set; } = new Dictionary<string, dynamic>();


        /// <summary>
        /// Gets the additional column filter options for a given filter type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IDictionary<string, dynamic> GetAdditionalColumnFilterOptions(Type type)
        {
            if (!_additionalColumnFilterOptions.ContainsKey(type))
            {
                _additionalColumnFilterOptions.Add(type, new Dictionary<string, dynamic>());
            }

            return _additionalColumnFilterOptions[type];
        }
    }
}
