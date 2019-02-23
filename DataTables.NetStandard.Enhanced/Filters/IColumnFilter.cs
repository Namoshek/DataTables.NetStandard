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
        object GetFilterOptions(int columnIndex);

        /// <summary>
        /// Gets or sets the data used to initialize the filter. For filters of type `select`,
        /// this can be a range of options for example.
        /// 
        /// Note: If, instead of a list of strings, a list of options is passed, they will be
        /// serialized as-they-are.
        /// </summary>
        IList<object> Data { get; set; }
    }
}
