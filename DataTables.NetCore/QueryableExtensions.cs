using System;
using System.Linq;

namespace DataTables.NetCore
{
    internal static class QueryableExtensions
    {
        public static IQueryable<TEntity> ApplyFilters<TEntity>(this IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
        {
            return filter.Invoke(query);
        }
    }
}
