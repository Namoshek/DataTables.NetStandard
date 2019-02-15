using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using DataTables.NetCore.Extensions;

namespace DataTables.NetCore
{
    /// <summary>
    /// Represents DataTables column filtering/sorting info
    /// </summary>
    /// <typeparam name="TEntity">Model type</typeparam>
    public class DataTablesColumn<TEntity, TEntityViewModel> : ICloneable
    {
        /// <summary>
        /// Column's index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Public name of this column.
        /// This will be the name used to serialize and deserialize the json data.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// The displayed name of the column in the actual DataTable.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Name of the private property (model) of this column.
        /// This property name cannot be changed by incoming requests.
        /// </summary>
        public string PrivatePropertyName { get; set; }

        /// <summary>
        /// Name of the public property (view model) of this column.
        /// </summary>
        public string PublicPropertyName { get; set; }

        /// <summary>
        /// Flag to indicate if this column is searchable (true) or not (false).
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Flag to indicate if this column is orderable (true) or not (false).
        /// </summary>
        public bool IsOrderable { get; set; }

        /// <summary>
        /// Search value to apply to this specific column.
        /// </summary>
        public string SearchValue { get; set; }

        /// <summary>
        /// Flag to indicate if the search term for this column should be treated as regular expression (true) or not (false).
        /// If this is set to false, no regex logic is applied even if the client requests it (for security reasons).
        /// Note: If a custom <see cref="ColumnSearchPredicate"/> or a <see cref="GlobalSearchPredicate"/> is used, 
        ///       implementing the regex logic has to be done invidually as well.
        /// 
        /// Note: If this option is set to false, then requests asking for regex evaluation will be handled as if they were non-regex 
        ///       requests. The reason for this is security as we only allow regex evaluation if the server-side agrees to it.
        ///       
        /// Note: As Linq queries with regex logic cannot be translated to native SQL queries, all queries with regex logic
        ///       will be evaluated in-memory, which is highly inefficient with large data sets. Be careful using this option.
        /// </summary>
        public bool SearchRegex { get; set; }

        /// <summary>
        /// Column's ordering index. The resulting enumerable should be sorted first by column with lowest ordering index.
        /// </summary>
        public int OrderingIndex { get; set; } = -1;

        /// <summary>
        /// Column's ordering direction, ascending or descending.
        /// </summary>
        public ListSortDirection OrderingDirection { get; set; }

        /// <summary>
        /// If true, the resulting enumerable will be sorted by the specified column with case-insensitive collation.
        /// </summary>
        public bool OrderingCaseInsensitive { get; set; }

        /// <summary>
        /// If true, the resulting enumerable will be searched by the specified column with case-insensitive collation.
        /// </summary>
        public bool SearchCaseInsensitive { get; set; }

        /// <summary>
        /// Optional predicate expression that will be used to search by the searchable column when <see cref="SearchValue"/> is specified. 
        /// If no predicate provided, <see cref="string.Contains(string)"/> method is used by default.
        /// </summary>
        public Expression<Func<TEntity, string, bool>> ColumnSearchPredicate { get; set; }

        /// <summary>
        /// Optional expression that specifies the different property which should be used if ordering by the column is required. 
        /// If no expression provided, the same property will be used for sorting as specified by <see cref="PrivatePropertyName"/> value.
        /// </summary>
        public Expression<Func<TEntity, object>> ColumnOrderingProperty { get; set; }

        /// <summary>
        /// Optional predicate expression that will be used to search by the searchable column when <see cref="DataTablesRequest{TEntity}.GlobalSearchValue"/> is specified. 
        /// If no predicate provided, <see cref="string.Contains(string)"/> method is used by default.
        /// </summary>
        public Expression<Func<TEntity, string, bool>> GlobalSearchPredicate { get; set; }

        /// <summary>
        /// A dictionary of additional options that should be passed to the DataTables script for this column.
        /// The entries are serialized as is and no PascalCase to camelCase transformation is performed.
        /// </summary>
        public IDictionary<string, dynamic> AdditionalOptions { get; set; } = new Dictionary<string, dynamic>();

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            var clone = (DataTablesColumn<TEntity, TEntityViewModel>)MemberwiseClone();

            clone.AdditionalOptions = clone.AdditionalOptions.DeepClone();

            return clone;
        }
    }
}
