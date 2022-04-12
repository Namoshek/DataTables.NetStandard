using System.Collections;

namespace DataTables.NetStandard.Abstract
{
    /// <summary>
    /// Describes a single page of data extracted from the <see cref="IDataTablesQueryable{TEntity}"/>.
    /// </summary>
    public interface IPagedList : IList
    {
        /// <summary>
        /// Total items count in the whole collection.
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Count of items per page.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// The number of the current page, starting from 1.
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Total number of pages in the whole collection.
        /// </summary>
        int PagesCount { get; }
    }
}
