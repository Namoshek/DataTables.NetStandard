using System.Collections.Generic;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public interface IColumnFilter
    {
        /// <summary>
        /// Returns an object that contains all the filter options of this filter.
        /// The object has to be serializable to JSON.
        /// </summary>
        /// <param name="columnIndex">The column index that should be used in the options.</param>
        FilterOptions GetFilterOptions(int columnIndex);
    }
}
