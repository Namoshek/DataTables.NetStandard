using System.Collections.Generic;
using System.Linq;
using DataTables.NetCore.Abstract;
using DataTables.NetCore.Extensions;

namespace DataTables.NetCore
{
    /// <summary>
    /// Base class for DataTables. Offers a default implementations for most required methods.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
    /// <seealso cref="Abstract.IDataTable{TEntity, TEntityViewModel}" />
    public abstract class DataTable<TEntity, TEntityViewModel> : IDataTable<TEntity, TEntityViewModel>
    {
        public abstract IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns();
        public abstract IQueryable<TEntity> Query();

        public DataTablesResponse<TEntity, TEntityViewModel> RenderResponse(string query)
        {
            var data = RenderResults(query);

            return new DataTablesResponse<TEntity, TEntityViewModel>(data, new DataTablesColumnsList<TEntity, TEntityViewModel>(Columns()));
        }

        public IPagedList<TEntityViewModel> RenderResults(string query)
        {
            var request = new DataTablesRequest<TEntity, TEntityViewModel>(query, new DataTablesColumnsList<TEntity, TEntityViewModel>(Columns()));

            return Query().ToPagedList(request);
        }

        public string RenderScript()
        {
            throw new System.NotImplementedException();
        }
    }
}
