using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class SelectFilter<TEntity> : BaseFilter, IFilterWithSelectableData<TEntity>
    {
        protected readonly Expression<Func<TEntity, LabelValuePair>> _keyValueSelector;

        public SelectFilter(Expression<Func<TEntity, LabelValuePair>> keyValueSelector)
        {
            _keyValueSelector = keyValueSelector;
        }

        public override string FilterType => "select";

        public IList<object> Data { get; set; }

        public Expression<Func<TEntity, LabelValuePair>> KeyValueSelector()
        {
            return _keyValueSelector;
        }

        public override FilterOptions GetFilterOptions(int columnIndex)
        {
            var options = base.GetFilterOptions(columnIndex);

            options.Data = Data;

            return options;
        }
    }
}
