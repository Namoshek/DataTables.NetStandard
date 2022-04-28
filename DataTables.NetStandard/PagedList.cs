using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.NetStandard.Abstract;

namespace DataTables.NetStandard
{
    /// <summary>
    /// Collection of items that represents a single page of data extracted from the <see cref="IDataTablesQueryable{TEntity, TEntityViewModel}"/>
    /// after applying <see cref="DataTablesRequest{TEntityViewModel}"/> filter.
    /// </summary>
    /// <typeparam name="TEntityViewModel">Data type</typeparam>
    public interface IPagedList<TEntityViewModel> : IPagedList, IList<TEntityViewModel> { }

    /// <summary>
    /// Internal implementation of <see cref="IPagedList{TEntity}"/> interface.
    /// </summary>
    /// <typeparam name="TEntity">Data type</typeparam>
    internal class PagedList<TEntity, TEntityViewModel> : List<TEntityViewModel>, IPagedList<TEntityViewModel>
    {
        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int PagesCount { get; }

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
        /// <param name="queryable"></param>
        /// <param name="request"></param>
        internal PagedList(IQueryable<TEntity> queryable, DataTablesRequest<TEntity, TEntityViewModel> request) : base()
        {
            PageNumber = request.PageNumber;
            PageSize = request.PageSize;
            TotalCount = queryable.Count();
            PagesCount = PageSize <= 0 ? 1 : (int)Math.Ceiling((double)(TotalCount / PageSize));

            int skipCount = Math.Abs((PageNumber - 1) * PageSize);
            int takeCount = PageSize <= 0 ? int.MaxValue : PageSize;

            var result = queryable.Skip(skipCount).Take(takeCount).ToList();

            var mapToViewModel = request.MappingFunction.Compile();
            AddRange(result.Select(e => mapToViewModel(e)));
        }
    }
}
