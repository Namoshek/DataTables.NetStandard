using System.Collections.Generic;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class TextInputFilter : BaseFilter, IColumnFilter
    {
        /// <summary>
        /// Defines the placeholder displayed on the filter.
        /// </summary>
        public string PlaceholderValue { get; set; }

        public override string FilterType => "text";

        internal TextInputFilter() { }

        public override FilterOptions GetFilterOptions(int columnIndex)
        {
            var options = base.GetFilterOptions(columnIndex, new Dictionary<string, dynamic>
            {
                { "filter_default_label", PlaceholderValue },
            });

            return options;
        }
    }
}
