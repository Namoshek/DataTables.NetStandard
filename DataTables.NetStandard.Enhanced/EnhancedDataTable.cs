using System.Collections.Generic;
using System.Linq;
using DataTables.NetStandard.Enhanced.Filters;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced
{
    public abstract class EnhancedDataTable<TEntity, TEntityViewModel> : DataTable<TEntity, TEntityViewModel>
    {
        /// <summary>
        /// Enhanced column definitions for this DataTable. Replaces normal column definitions.
        /// </summary>
        public abstract IList<EnhancedDataTablesColumn<TEntity, TEntityViewModel>> EnhancedColumns();

        /// <summary>
        /// Additional filter options that are used for each column if not overridden.
        /// Used as filter options for the yadcf DataTables filter system.
        /// </summary>
        public virtual IDictionary<string, dynamic> AdditionalColumnFilterOptions()
        {
            return new Dictionary<string, dynamic>();
        }

        /// <summary>
        /// Additional filter options used to initialize the yadcf DataTables filter system.
        /// </summary>
        public virtual IDictionary<string, dynamic> AdditionalFilterOptions()
        {
            return new Dictionary<string, dynamic>();
        }

        /// <summary>
        /// We simply forward our enhanced column definitions as normal column definitions.
        /// This is possible because they still work the same except that they hold additional data.
        /// </summary>
        public override sealed IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns()
        {
            return EnhancedColumns().Cast<DataTablesColumn<TEntity, TEntityViewModel>>().ToList();
        }

        /// <summary>
        /// Renders the script.
        /// </summary>
        /// <param name="url">The url of the data endpoint for the DataTable</param>
        /// <param name="method">The http method used for the data endpoint (get or post)</param>
        public override string RenderScript(string url, string method = "get")
        {
            var script = base.RenderScript(url, method);

            var columns = EnhancedColumns();

            // Load data for select filters
            // TODO: generalize
            columns.Where(c => c.ColumnFilter is IFilterWithData)
                .ToList()
                .ForEach(c => c.ColumnFilter.Data = GetDistinctColumnValues(c.PublicName).Cast<object>().ToList());

            var columnFilters = columns.Where(c => c.ColumnFilter != null)
                .Select(c => c.ColumnFilter.GetFilterOptions(columns.IndexOf(c)))
                .ToList();

            var columnOptions = JsonConvert.SerializeObject(columnFilters);
            var globalOptions = JsonConvert.SerializeObject(AdditionalFilterOptions());

            // We reference the DataTable instance with <code>dt_{tableIdentifier}</code>
            script += $"yadcf.init(dt_{GetTableIdentifier()}, {columnOptions}, {globalOptions});";

            return script;
        }
    }
}
