using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataTables.NetStandard.Sample.DataTables
{
    public abstract class BaseDataTable<TEntity, TEntityViewModel> : DataTable<TEntity, TEntityViewModel>
    {
        public override Expression<Func<TEntity, TEntityViewModel>> MappingFunction()
        {
            return e => AutoMapper.Mapper.Map<TEntityViewModel>(e);
        }

        protected override void ConfigureAdditionalOptions(DataTablesConfiguration configuration, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
        {
            configuration.AdditionalOptions["stateSave"] = false;
        }
    }
}
