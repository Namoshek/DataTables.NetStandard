using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class SelectFilter<TEntity> : BaseFilter, IFilterWithSelectableData<TEntity>
    {
        protected readonly Expression<Func<TEntity, LabelValuePair>> _keyValueSelector;

        public IList<object> Data { get; set; }

        /// <summary>
        /// Defines whether the filter should display a default label like 'Select value'.
        /// </summary>
        public bool? EnableDefaultSelectionLabel { get; set; }

        /// <summary>
        /// Defines the default label displayed on the filter if <see cref="EnableDefaultSelectionLabel"/>
        /// is enabled. Can be used to localize the filters.
        /// </summary>
        public string DefaultSelectionLabelValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFilter{TEntity}"/> class.
        /// </summary>
        /// <param name="keyValueSelector">A selector used to load the filter options.</param>
        internal SelectFilter(Expression<Func<TEntity, LabelValuePair>> keyValueSelector)
        {
            _keyValueSelector = keyValueSelector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFilter{TEntity}"/> class.
        /// </summary>
        /// <param name="filterOptions">The filter options.</param>
        internal SelectFilter(IList<LabelValuePair> filterOptions)
        {
            Data = filterOptions.Cast<object>().ToList();
        }

        public override string FilterType => "select";

        public Expression<Func<TEntity, LabelValuePair>> KeyValueSelector()
        {
            return _keyValueSelector;
        }

        public override FilterOptions GetFilterOptions(int columnIndex)
        {
            var options = base.GetFilterOptions(columnIndex, new Dictionary<string, dynamic>
            {
                { "omit_default_label", !EnableDefaultSelectionLabel },
                { "filter_default_label", DefaultSelectionLabelValue },
            });

            options.Data = Data;

            return options;
        }
    }
}
