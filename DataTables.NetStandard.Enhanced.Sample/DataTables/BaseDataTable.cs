using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DataTables.NetStandard.Enhanced.Filters;

namespace DataTables.NetStandard.Enhanced.Sample.DataTables
{
    public abstract class BaseDataTable<TEntity, TEntityViewModel> : EnhancedDataTable<TEntity, TEntityViewModel>
    {
        public override Expression<Func<TEntity, TEntityViewModel>> MappingFunction()
        {
            return e => AutoMapper.Mapper.Map<TEntityViewModel>(e);
        }

        protected override void ConfigureFilters(DataTablesFilterConfiguration configuration)
        {
            configuration.DefaultSelectionLabelValue = "Select something";
            configuration.DefaultTextInputPlaceholderValue = "Type to find";

            configuration.AdditionalFilterOptions.Add("filters_position", "footer");

            var selectFilterConfiguration = configuration.GetAdditionalColumnFilterOptions(typeof(SelectFilter<TEntity>));
            selectFilterConfiguration["sort_as"] = "alphaNum";
        }

        protected override void ConfigureAdditionalOptions(DataTablesConfiguration configuration, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
        {
            configuration.AdditionalOptions["stateSave"] = false;
            configuration.AdditionalOptions["colReorder"] = true;
            configuration.AdditionalOptions["scrollX"] = true;
            configuration.AdditionalOptions["pagingType"] = "full_numbers";
            configuration.AdditionalOptions["search"] = new { smart = true };
        }
    }
}
