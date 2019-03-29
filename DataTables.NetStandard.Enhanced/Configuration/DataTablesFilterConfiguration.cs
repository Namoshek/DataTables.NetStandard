namespace DataTables.NetStandard.Enhanced.Configuration
{
    public class DataTablesFilterConfiguration
    {
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
    }
}
