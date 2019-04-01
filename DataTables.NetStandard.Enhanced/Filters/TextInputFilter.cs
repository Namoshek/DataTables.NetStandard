using System.Collections.Generic;
using DataTables.NetStandard.Enhanced.Configuration;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class TextInputFilter : BaseFilter, IColumnFilter
    {
        /// <summary>
        /// Defines the placeholder displayed on the filter.
        /// </summary>
        public string PlaceholderValue { get; set; } = null;

        public override string FilterType => "text";

        public override FilterOptions GetFilterOptions(int columnIndex)
        {
            var options = base.GetFilterOptions(columnIndex, new Dictionary<string, dynamic>
            {
                { "filter_default_label", PlaceholderValue ?? EnhancedDataTablesConfiguration.FilterConfiguration.DefaultTextInputPlaceholderValue },
            });

            return options;
        }
    }
}
