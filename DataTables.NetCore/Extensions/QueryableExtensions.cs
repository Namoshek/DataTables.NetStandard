using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataTables.NetCore.Util;

namespace DataTables.NetCore.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Builds a <see cref="PagedList{TEntity, TEntityViewModel}"/> from the given <see cref="IQueryable{T}"/>.
        /// Before building the paged list, all filters and options of <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>
        /// will be applied to the given <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="request">The request.</param>
        public static IPagedList<TEntityViewModel> ToPagedList<TEntity, TEntityViewModel>(this IQueryable<TEntity> queryable, 
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return queryable.Apply(request).ToPagedList();
        }

        /// <summary>
        /// Builds a <see cref="PagedList{TEntity, TEntityViewModel}"/> from the given <see cref="IQueryable{T}"/> asynchronously.
        /// Before building the paged list, all filters and options of <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>
        /// will be applied to the given <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="request">The request.</param>
        public static Task<IPagedList<TEntityViewModel>> ToPagedListAsync<TEntity, TEntityViewModel>(this IQueryable<TEntity> queryable, 
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return queryable.Apply(request).ToPagedListAsync();
        }

        /// <summary>
        /// Applies the given <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/> to the given
        /// <see cref="IQueryable{T}"/> by performing a few consecutive steps. The method will apply
        /// the global filter predicate, global search, column search and column ordering.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="request">The request.</param>
        public static IDataTablesQueryable<TEntity, TEntityViewModel> Apply<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable, DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            queryable = queryable.AsDataTablesQueryable(request)
                .ApplyGlobalSearchFilter()
                .ApplyColumnSearchFilter()
                .ApplyOrder();

            if (request.Log != null)
            {
                var sb = new StringBuilder("DataTables.Queryable -> Incoming request:\n");

                foreach (string key in request.OriginalRequest.AllKeys)
                {
                    string value = request.OriginalRequest[key];
                    sb.AppendLine($"{key} = {$"\"{value}\""}");
                }

                sb.AppendLine();
                sb.AppendLine($"DataTables.Queryable -> Resulting queryable:\n{queryable}\n");

                request.Log.Invoke(sb.ToString());
            }

            return (IDataTablesQueryable<TEntity, TEntityViewModel>)queryable;
        }

        /// <summary>
        /// Converts the given <see cref="IQueryable{T}"/> and the given <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>
        /// to a new <see cref="DataTablesQueryable{TEntity, TEntityViewModel}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="request">The request.</param>
        public static IDataTablesQueryable<TEntity, TEntityViewModel> AsDataTablesQueryable<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable, DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return new DataTablesQueryable<TEntity, TEntityViewModel>(queryable, request);
        }

        /// <summary>
        /// Orders the given <see cref="IQueryable{TEntity}"/> by the given <paramref name="propertyName"/>.
        /// The method will consider the given <paramref name="direction"/> and if the order should be applied
        /// with <paramref name="caseInsensitive"/> logic.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="caseInsensitive">if set to <c>true</c>, the order logic is case insensitive.</param>
        /// <param name="alreadyOrdered">if set to <c>true</c>, follow-up order logic will be used.</param>
        internal static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, 
            string propertyName, ListSortDirection direction, bool caseInsensitive, bool alreadyOrdered)
        {
            var type = typeof(TEntity);
            var parameterExp = Expression.Parameter(type, "e");
            var propertyExp = ExpressionHelper.BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            if (caseInsensitive && propertyExp.Type == typeof(string))
            {
                exp = Expression.Call(exp, ExpressionHelper.String_ToLower);
            }

            var methodName = GetOrderMethodName(direction, alreadyOrdered);
            var orderByExp = Expression.Lambda(exp, parameterExp);
            var typeArguments = new Type[] { type, propertyExp.Type };

            var resultExpr = Expression.Call(typeof(Queryable), methodName, typeArguments, query.Expression, Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<TEntity>(resultExpr);
        }

        /// <summary>
        /// Gets the name of the order method matching the given requirements.
        /// </summary>
        /// <param name="direction">The order direction.</param>
        /// <param name="alreadyOrdered">if set to <c>true</c>, an order method for already ordered queryables will be returned.</param>
        /// <returns></returns>
        internal static string GetOrderMethodName(ListSortDirection direction, bool alreadyOrdered)
        {
            if (direction == ListSortDirection.Descending && !alreadyOrdered)
                return nameof(Queryable.OrderByDescending);

            if (direction == ListSortDirection.Ascending && alreadyOrdered)
                return nameof(Queryable.ThenBy);

            if (direction == ListSortDirection.Descending && alreadyOrdered)
                return nameof(Queryable.ThenByDescending);

            // direction == ListSortDirection.Ascending && alreadyOrdered == false
            return nameof(Queryable.OrderBy);
        }
    }
}
