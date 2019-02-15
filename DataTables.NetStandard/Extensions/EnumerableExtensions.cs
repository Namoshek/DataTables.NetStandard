using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataTables.NetStandard.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Builds a <see cref="PagedList{TEntity, TEntityViewModel}"/> from the given <see cref="IEnumerable{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="request">The request.</param>
        public static IPagedList<TEntityViewModel> ToPagedList<TEntity, TEntityViewModel>(
            this IEnumerable<TEntity> source, DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return source.AsQueryable().ToPagedList(request);
        }

        /// <summary>
        /// Builds a <see cref="PagedList{TEntity, TEntityViewModel}"/> from the given <see cref="IEnumerable{TEntity}"/> asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="request">The request.</param>
        public static Task<IPagedList<TEntityViewModel>> ToPagedListAsync<TEntity, TEntityViewModel>(
            this IEnumerable<TEntity> source, DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return Task.Factory.StartNew(() => source.AsQueryable().ToPagedList(request));
        }
    }
}
