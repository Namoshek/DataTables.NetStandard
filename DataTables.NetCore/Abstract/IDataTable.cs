using System.Linq;

namespace DataTables.NetCore.Abstract
{
    public interface IDataTable<TEntity, TEntityViewModel>
    {
        /// <summary>
        /// Column definitions for this DataTable.
        /// </summary>
        IDataTablesColumnsCollection<TEntity, TEntityViewModel> Columns();

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
        /// <param name="url">The url of the data endpoint for the DataTable</param>
        /// <param name="method">The http method used for the data endpoint (get or post)</param>
        string RenderScript(string url, string method);

        /// <summary>
        /// Renders the HTML.
        /// </summary>
        /// <returns></returns>
        string RenderHtml();
    }
}
