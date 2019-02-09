using System.Collections.Generic;
using System.Linq;

namespace DataTables.NetCore.Abstract
{
    public interface IDataTable<TEntity, TEntityViewModel>
    {
        /// <summary>
        /// Column definitions for this DataTable.
        /// </summary>
        IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns();

        /// <summary>
        /// Gets the query used to fetch data for the DataTable.
        /// </summary>
        IQueryable<TEntity> Query();

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{TEntityViewModel}"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        IPagedList<TEntityViewModel> RenderResults(string query);

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{TEntityViewModel}"/> and builds a response
        /// that can be returned immediately.
        /// </summary>
        /// <param name="query">The query.</param>
        DataTablesResponse<TEntity, TEntityViewModel> RenderResponse(string query);

        /// <summary>
        /// Renders the script.
        /// </summary>
        string RenderScript();
    }
}
