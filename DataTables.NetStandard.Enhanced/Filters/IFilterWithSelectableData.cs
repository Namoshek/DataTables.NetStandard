using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataTables.NetStandard.Enhanced.Filters
{
    internal interface IFilterWithSelectableData<TEntity>
    {
        /// <summary>
        /// Gets or sets the data used to initialize the filter. For filters of type `select`,
        /// this can be a range of options for example.
        /// 
        /// Note: If, instead of a list of strings, a list of options is passed, they will be
        /// serialized as-they-are.
        /// </summary>
        IList<object> Data { get; set; }

        /// <summary>
        /// Expression used to select selection filter data.
        /// </summary>
        Expression<Func<TEntity, LabelValuePair>> KeyValueSelector();
    }
}
