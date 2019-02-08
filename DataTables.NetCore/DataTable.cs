using System.Linq;
using System.Reflection;
using AutoMapper;
using DataTables.NetCore.Abstract;
using DataTables.NetCore.Attributes;
using DataTables.NetCore.Extensions;

namespace DataTables.NetCore
{
    public abstract class DataTable<TEntity, TEntityViewModel> : IDataTable<TEntity, TEntityViewModel>
    {
        public abstract IQueryable<TEntity> Query();

        public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query)
        {
            return query;
        }

        public DataTablesResponse<TEntityViewModel> RenderResponse(DataTablesRequest<TEntity> request)
        {
            var data = RenderResults(request);

            return new DataTablesResponse<TEntityViewModel>(data);
        }

        public IPagedList<TEntityViewModel> RenderResults(DataTablesRequest<TEntity> request)
        {
            var viewModelMembers = typeof(TEntityViewModel).GetMembers();

            foreach (var column in request.Columns)
            {
                var member = viewModelMembers.FirstOrDefault(m => m.GetCustomAttribute<DTColumn>()?.Data == column.PropertyName);
                if (member != null)
                {
                    column.PropertyName = member.GetCustomAttribute<DTColumn>().QueryName;
                }
            }

            return Query().ApplyFilters(Filter).ToPagedList(request).Convert(e => Mapper.Map<TEntity, TEntityViewModel>(e));
        }

        public string RenderScript()
        {
            throw new System.NotImplementedException();
        }
    }
}
