using System.Linq;
using DataTables.Queryable;

namespace DataTables.NetCore
{
    public interface IDataTable<TEntity, TEntityViewModel>
    {
        /// <summary>
        /// Gets the query used to fetch data for the DataTable.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        IQueryable<TEntity> Query();

        /// <summary>
        /// Applies additional filters to the DataTable.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        IQueryable<TEntity> Filter(IQueryable<TEntity> query);

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{T}"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        IPagedList<TEntityViewModel> RenderResults(DataTablesRequest<TEntity> request);

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{T}"/> and builds a response
        /// that can be returned immediately.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        DataTableResponse<TEntityViewModel> RenderResponse(DataTablesRequest<TEntity> request);

        /// <summary>
        /// Renders the script.
        /// </summary>
        /// <returns></returns>
        string RenderScript();
    }
}
