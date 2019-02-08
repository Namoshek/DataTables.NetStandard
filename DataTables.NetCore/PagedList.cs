using System.Collections.Generic;
using System.Linq;
using DataTables.NetCore.Abstract;

namespace DataTables.NetCore
{
    /// <summary>
    /// Collection of items that represents a single page of data extracted from the <see cref="IDataTablesQueryable{TEntity}"/>
    /// after applying <see cref="DataTablesRequest{TEntity}"/> filter.
    /// </summary>
    /// <typeparam name="TEntity">Data type</typeparam>
    public interface IPagedList<TEntity> : IPagedList, IList<TEntity> { }

    /// <summary>
    /// Internal implementation of <see cref="IPagedList{TEntity}"/> interface.
    /// </summary>
    /// <typeparam name="TEntity">Data type</typeparam>
    internal class PagedList<TEntity> : List<TEntity>, IPagedList<TEntity>
    {
        public int TotalCount { get; protected set; }
        public int PageNumber { get; protected set; }
        public int PageSize { get; protected set; }
        public int PagesCount { get; protected set; }

        internal PagedList(IPagedList other) : base()
        {
            TotalCount = other.TotalCount;
            PageNumber = other.PageNumber;
            PageSize = other.PageSize;
            PagesCount = other.PagesCount;
        }

        /// <summary>
        /// Creates new instance of <see cref="PagedList{TEntity}"/> collection.
        /// </summary>
        /// <param name="queryable"><see cref="IDataTablesQueryable{TEntity}"/>instance to be paginated</param>
        internal PagedList(IDataTablesQueryable<TEntity> queryable) : base()
        {
            // pagination is on
            if (queryable.Request.PageSize > 0)
            {
                int skipCount = (queryable.Request.PageNumber - 1) * queryable.Request.PageSize;
                int takeCount = queryable.Request.PageSize;

                TotalCount = queryable.Count();
                PageNumber = queryable.Request.PageNumber;
                PageSize = queryable.Request.PageSize;
                PagesCount = TotalCount % PageSize == 0 ? TotalCount / PageSize : TotalCount / PageSize + 1;

                AddRange(queryable.Skip(skipCount).Take(takeCount).ToList());
            }
            // no pagination
            else
            {
                TotalCount = queryable.Count();
                PageNumber = 1;
                PageSize = -1;
                PagesCount = 1;

                AddRange(queryable.ToList());
            }
        }
    }
}
