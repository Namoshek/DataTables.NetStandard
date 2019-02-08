using System.Linq;
using AutoMapper;
using DataTables.Queryable;

namespace DataTables.NetCore
{
    public abstract class DataTable<TEntity, TEntityViewModel> : IDataTable<TEntity, TEntityViewModel>
    {
        public abstract IQueryable<TEntity> Query();

        public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query)
        {
            return query;
        }

        public DataTableResponse<TEntityViewModel> RenderResponse(DataTablesRequest<TEntity> request)
        {
            var data = RenderResults(request);

            return new DataTableResponse<TEntityViewModel>(data);
        }

        public IPagedList<TEntityViewModel> RenderResults(DataTablesRequest<TEntity> request)
        {
            return Query().ApplyFilters(Filter).ToPagedList(request).Convert(e => Mapper.Map<TEntity, TEntityViewModel>(e));
        }

        public string RenderScript()
        {
            throw new System.NotImplementedException();
        }
    }
}
