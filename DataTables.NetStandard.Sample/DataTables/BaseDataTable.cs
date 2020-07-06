using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataTables.NetStandard.Sample.DataTables
{
    public abstract class BaseDataTable<TEntity, TEntityViewModel> : DataTable<TEntity, TEntityViewModel>
    {
        protected readonly IMapper _mapper;

        protected BaseDataTable(IMapper mapper)
        {
            _mapper = mapper;
        }

        public override Expression<Func<TEntity, TEntityViewModel>> MappingFunction()
        {
            return entity => _mapper.Map<TEntityViewModel>(entity);
        }

        protected override void ConfigureAdditionalOptions(DataTablesConfiguration configuration, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
        {
            configuration.AdditionalOptions["stateSave"] = false;
        }
    }
}
