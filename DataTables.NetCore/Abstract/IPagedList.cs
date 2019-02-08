using System.Collections;

namespace DataTables.NetCore.Abstract
{
    /// <summary>
    /// Describes single page of data extracted from the <see cref="IDataTablesQueryable{TEntity}"/> 
    /// </summary>
    public interface IPagedList : IList
    {
        /// <summary>
        /// Total items count in the whole collection 
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Count of items per page
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// 1-bazed page number
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Total number of pages in the whole collection
        /// </summary>
        int PagesCount { get; }
    }
}
