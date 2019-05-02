using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataTables.NetStandard.Enhanced.Filters;
using DataTables.NetStandard.Extensions;
using MoreLinq.Extensions;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced
{
    public abstract class EnhancedDataTable<TEntity, TEntityViewModel> : DataTable<TEntity, TEntityViewModel>
    {
        protected DataTablesFilterConfiguration _filterConfiguration;

        public EnhancedDataTable()
        {
            _filterConfiguration = new DataTablesFilterConfiguration();
        }

        /// <summary>
        /// Enhanced column definitions for this DataTable. Replaces normal column definitions.
        /// </summary>
        public abstract IList<EnhancedDataTablesColumn<TEntity, TEntityViewModel>> EnhancedColumns();

        /// <summary>
        /// We simply forward our enhanced column definitions as normal column definitions.
        /// This is possible because they still work the same except that they hold additional data.
        /// </summary>
        public override sealed IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns()
        {
            return EnhancedColumns().Cast<DataTablesColumn<TEntity, TEntityViewModel>>().ToList();
        }

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>
        /// and builds a response that can be returned immediately.
        /// </summary>
        /// <param name="query">The query.</param>
        public override DataTablesResponse<TEntity, TEntityViewModel> RenderResponse(string query)
        {
            Configure();

            var request = BuildRequest(query);
            var data = RenderResults(request);

            var columnFilters = GetColumnFilterOptions(request);
            var filterData = columnFilters.ToDictionary(f => $"yadcf_data_{f.ColumnNumber}", f => f.Data as dynamic);

            return new EnhancedDataTablesResponse<TEntity, TEntityViewModel>(data,
                Columns(),
                request.Draw,
                filterData);
        }

        /// <summary>
        /// Renders the script.
        /// </summary>
        /// <param name="url">The url of the data endpoint for the DataTable</param>
        /// <param name="method">The http method used for the data endpoint (get or post)</param>
        public override string RenderScript(string url, string method = "get")
        {
            Configure();

            var script = base.RenderScript(url, method);
            var columnFilters = GetColumnFilterOptions();

            var columnOptions = JsonConvert.SerializeObject(columnFilters);
            var globalOptions = JsonConvert.SerializeObject(_filterConfiguration.AdditionalFilterOptions);

            // We reference the DataTable instance with <code>dt_{tableIdentifier}</code>
            script += $"yadcf.init(dt_{GetTableIdentifier()}, {columnOptions}, {globalOptions});";

            return script;
        }

        /// <summary>
        /// Creates a new text input filter based on the default configuration.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual TextInputFilter CreateTextInputFilter(Action<TextInputFilter> configure = null)
        {
            var filter = new TextInputFilter
            {
                PlaceholderValue = _filterConfiguration.DefaultTextInputPlaceholderValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(TextInputFilter)),
            };

            configure?.Invoke(filter);

            return filter;
        }

        /// <summary>
        /// Creates a new select filter based on the default configuration and the given key value selector.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="keyValueSelector"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual SelectFilter<TEntity> CreateSelectFilter(Expression<Func<TEntity, LabelValuePair>> keyValueSelector, 
            Action<SelectFilter<TEntity>> configure = null)
        {
            var filter = new SelectFilter<TEntity>(keyValueSelector)
            {
                EnableDefaultSelectionLabel = _filterConfiguration.EnableDefaultSelectionLabel,
                DefaultSelectionLabelValue = _filterConfiguration.DefaultSelectionLabelValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(SelectFilter<TEntity>)),
            };

            configure?.Invoke(filter);

            return filter;
        }

        /// <summary>
        /// Creates a new select filter based on the default configuration and the given filter options.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="filterOptions"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual SelectFilter<TEntity> CreateSelectFilter(IList<LabelValuePair> filterOptions, 
            Action<SelectFilter<TEntity>> configure = null)
        {
            var filter = new SelectFilter<TEntity>(filterOptions)
            {
                EnableDefaultSelectionLabel = _filterConfiguration.EnableDefaultSelectionLabel,
                DefaultSelectionLabelValue = _filterConfiguration.DefaultSelectionLabelValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(SelectFilter<TEntity>)),
            };

            configure?.Invoke(filter);

            return filter;
        }

        /// <summary>
        /// Returns a list of distinct column values that can be used for select filters.
        /// </summary>
        /// <param name="property"></param>
        public virtual IList<LabelValuePair> GetDistinctColumnValuesForSelect(Expression<Func<TEntity, LabelValuePair>> selector,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            var query = Query();

            if (request != null)
            {
                query = query.Apply(request);
            }

            // TODO: As soon as EFCore supports translation of .GroupBy(expr).Select(e => e.FirstOrDefault()),
            //       this method can be improved significantly by applying the group by instead of distinctby.
            return query.Select(selector.Compile())
                .DistinctBy(e => e.Value)
                .ToList();
        }

        /// <summary>
        /// Allows to configure the DataTable instance.
        /// </summary>
        protected override void Configure()
        {
            if (_isConfigured)
            {
                return;
            }

            var columns = Columns();

            ConfigureColumns(_configuration, columns);
            ConfigureColumnOrdering(_configuration, columns);
            ConfigureAdditionalOptions(_configuration, columns);
            ConfigureFilters(_filterConfiguration);

            _isConfigured = true;
        }

        /// <summary>
        /// Allows to configure the DataTable filters.
        /// </summary>
        /// <param name="configuration"></param>
        protected virtual void ConfigureFilters(DataTablesFilterConfiguration configuration)
        {
            // We do not configure anything, but we provide a default implementation.
        }

        /// <summary>
        /// Returns a list of prefilled column filters with their corresponding options.
        /// </summary>
        protected virtual IList<FilterOptions> GetColumnFilterOptions(DataTablesRequest<TEntity, TEntityViewModel> request = null)
        {
            var columns = EnhancedColumns();

            columns.Where(c => c.ColumnFilter is IFilterWithSelectableData<TEntity> && (c.ColumnFilter as IFilterWithSelectableData<TEntity>).Data == null)
                .ToList()
                .ForEach(c =>
                {
                    var col = c.ColumnFilter as IFilterWithSelectableData<TEntity>;
                    if (col.KeyValueSelector() != null)
                    {
                        col.Data = GetDistinctColumnValuesForSelect(col.KeyValueSelector(), request).Cast<object>().ToList();
                    }
                });

            return columns.Where(c => c.ColumnFilter != null)
                .Select(c => c.ColumnFilter.GetFilterOptions(columns.IndexOf(c)))
                .ToList();
        }
    }
}
