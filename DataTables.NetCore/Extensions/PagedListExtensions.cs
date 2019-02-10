using System;

namespace DataTables.NetCore.Extensions
{
    public static class PagedListExtensions
    {
        /// <summary>
        /// Applies the specified action to each element of the <see cref="IPagedList{TEntityViewModel}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="action">The action.</param>
        public static IPagedList<TEntity> Apply<TEntity>(this IPagedList<TEntity> list, Action<TEntity> action)
        {
            foreach (var item in list)
            {
                action(item);
            }

            return list;
        }
    }
}
