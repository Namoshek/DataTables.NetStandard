using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataTables.NetStandard.Util;

namespace DataTables.NetStandard.Extensions
{
    public static partial class QueryableExtensions
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
        public static IPagedList<TEntityViewModel> ToPagedList<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return new PagedList<TEntity, TEntityViewModel>(queryable.Apply(request), request);
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
        public static Task<IPagedList<TEntityViewModel>> ToPagedListAsync<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            return Task.Factory.StartNew<IPagedList<TEntityViewModel>>(() => new PagedList<TEntity, TEntityViewModel>(queryable.Apply(request), request));
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
        public static IQueryable<TEntity> Apply<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            queryable = queryable
                .ApplyGlobalSearchFilter(request)
                .ApplyColumnSearchFilter(request)
                .ApplyOrder(request);

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

            return queryable;
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
        internal static IQueryable<TEntity> OrderBy<TEntity>(
            this IQueryable<TEntity> query,
            string propertyName,
            ListSortDirection direction,
            bool caseInsensitive,
            bool alreadyOrdered)
        {
            var parameterExp = ExpressionHelper.BuildParameterExpression<TEntity>();
            var propertyExp = ExpressionHelper.BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            if (caseInsensitive && propertyExp.Type == typeof(string))
            {
                exp = Expression.Call(exp, ExpressionHelper.String_ToLower);
            }

            var methodName = GetOrderMethodName(direction, alreadyOrdered);
            var orderByExp = Expression.Lambda(exp, parameterExp);
            var typeArguments = new Type[] { typeof(TEntity), propertyExp.Type };

            var resultExpr = Expression.Call(typeof(Queryable), methodName, typeArguments, query.Expression, Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<TEntity>(resultExpr);
        }

        /// <summary>
        /// Orders the given <see cref="IQueryable{TEntity}"/> by the given <paramref name="propertyName"/>.
        /// The method will consider the given <paramref name="direction"/> and if the order should be applied
        /// with <paramref name="caseInsensitive"/> logic.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="orderExpression">Name of the property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="alreadyOrdered">if set to <c>true</c>, follow-up order logic will be used.</param>
        internal static IQueryable<TEntity> OrderBy<TEntity>(
            this IQueryable<TEntity> query,
            Expression<Func<TEntity, object>> orderExpression,
            ListSortDirection direction,
            bool alreadyOrdered)
        {
            var methodName = GetOrderMethodName(direction, alreadyOrdered);
            var typeArguments = new Type[] { typeof(TEntity), typeof(object) };

            var resultExpr = Expression.Call(typeof(Queryable), methodName, typeArguments, query.Expression, orderExpression);

            return query.Provider.CreateQuery<TEntity>(resultExpr);
        }

        /// <summary>
        /// Gets the name of the order method matching the given requirements.
        /// </summary>
        /// <param name="direction">The order direction.</param>
        /// <param name="alreadyOrdered">if set to <c>true</c>, an order method for already ordered queryables will be returned.</param>
        internal static string GetOrderMethodName(ListSortDirection direction, bool alreadyOrdered)
        {
            return (direction, alreadyOrdered) switch
            {
                (ListSortDirection.Descending, false) => nameof(Queryable.OrderByDescending),
                (ListSortDirection.Ascending, true) => nameof(Queryable.ThenBy),
                (ListSortDirection.Descending, true) => nameof(Queryable.ThenByDescending),

                _ => nameof(Queryable.OrderBy),
            };
        }

        /// <summary>
        /// Applies the <see cref="DataTablesRequest{TEntity, TEntityViewModel}.GlobalSearchValue"/> of the given
        /// <see cref="IQueryable{TEntity, TEntityViewModel}"/> to the query, if set.
        /// To perform the global search, the method will add a search expression for each searchable column of the request.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable"></param>
        /// <param name="request"></param>
        internal static IQueryable<TEntity> ApplyGlobalSearchFilter<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            var globalSearchValue = request.GlobalSearchValue;
            if (!string.IsNullOrEmpty(globalSearchValue))
            {
                var columns = request.Columns.Where(c => c.IsSearchable);

                if (columns.Any())
                {
                    Expression<Func<TEntity, bool>> predicate = null;

                    foreach (var c in columns)
                    {
                        var globalSearchPredicateByProvider = c.GlobalSarchPredicateProvider != null
                            ? c.GlobalSarchPredicateProvider(globalSearchValue)
                            : null;

                        var searchPredicateByProvider = globalSearchPredicateByProvider == null && c.SearchPredicateProvider != null
                            ? c.SearchPredicateProvider(globalSearchValue)
                            : null;

                        Expression<Func<TEntity, bool>> expression;

                        var searchPredicate = globalSearchPredicateByProvider
                            ?? searchPredicateByProvider
                            ?? c.GlobalSearchPredicate
                            ?? c.SearchPredicate;

                        if (searchPredicate != null)
                        {
                            var expr = searchPredicate;
                            var entityParam = ExpressionHelper.BuildParameterExpression<TEntity>();
                            var searchValueConstant = ExpressionHelper.CreateConstantFilterExpression(globalSearchValue, typeof(string));
                            expression = (Expression<Func<TEntity, bool>>)Expression.Lambda(
                                Expression.Invoke(expr, entityParam, searchValueConstant),
                                entityParam);
                        }
                        else if (request.GlobalSearchRegex)
                        {
                            expression = ExpressionHelper.BuildRegexPredicate<TEntity>(c.PrivatePropertyName, globalSearchValue);
                        }
                        else
                        {
                            expression = ExpressionHelper.BuildStringContainsPredicate<TEntity>(
                                c.PrivatePropertyName,
                                globalSearchValue,
                                c.SearchCaseInsensitive);
                        }

                        predicate = predicate == null
                            ? expression
                            : predicate.Or(expression);
                    }

                    if (predicate != null)
                    {
                        queryable = queryable.Where(predicate);
                    }
                }
            }

            return queryable;
        }

        /// <summary>
        /// Applies the search filter for each of the searchable <see cref="DataTablesRequest{TEntity, TEntityViewModel}.Columns"/>
        /// where a search value is present.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable"></param>
        /// <param name="request"></param>
        internal static IQueryable<TEntity> ApplyColumnSearchFilter<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            var columns = request.Columns
                .Where(c => c.IsSearchable && !string.IsNullOrEmpty(c.SearchValue));

            if (columns.Any())
            {
                Expression<Func<TEntity, bool>> predicate = null;

                foreach (var c in columns)
                {
                    var columnSearchPredicateByProvider = c.ColumnSearchPredicateProvider != null
                        ? c.ColumnSearchPredicateProvider(c.SearchValue)
                        : null;

                    var searchPredicateByProvider = columnSearchPredicateByProvider == null && c.SearchPredicateProvider != null
                        ? c.SearchPredicateProvider(c.SearchValue)
                        : null;

                    Expression<Func<TEntity, bool>> expression;

                    var searchPredicate = columnSearchPredicateByProvider
                        ?? searchPredicateByProvider
                        ?? c.ColumnSearchPredicate
                        ?? c.SearchPredicate;

                    if (searchPredicate != null)
                    {
                        var expr = searchPredicate;
                        var entityParam = ExpressionHelper.BuildParameterExpression<TEntity>();
                        var searchValueConstant = ExpressionHelper.CreateConstantFilterExpression(c.SearchValue, typeof(string));
                        expression = (Expression<Func<TEntity, bool>>)Expression.Lambda(
                            Expression.Invoke(expr, entityParam, searchValueConstant),
                            entityParam);
                    }
                    else if (c.SearchRegex)
                    {
                        expression = ExpressionHelper.BuildRegexPredicate<TEntity>(c.PrivatePropertyName, c.SearchValue);
                    }
                    else
                    {
                        expression = ExpressionHelper.BuildStringContainsPredicate<TEntity>(
                            c.PrivatePropertyName,
                            c.SearchValue,
                            c.SearchCaseInsensitive);
                    }

                    predicate = predicate == null
                        ? expression
                        : predicate.And(expression);
                }

                if (predicate != null)
                {
                    queryable = queryable.Where(predicate);
                }
            }

            return queryable;
        }

        /// <summary>
        /// Applies the requested order for each of the <see cref="DataTablesRequest{TEntity, TEntityViewModel}.Columns"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable"></param>
        /// <param name="request"></param>
        internal static IQueryable<TEntity> ApplyOrder<TEntity, TEntityViewModel>(
            this IQueryable<TEntity> queryable,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            var columns = request.Columns
                .Where(c => c.IsOrderable && c.OrderingIndex >= 0)
                .OrderBy(c => c.OrderingIndex);

            bool alreadyOrdered = false;

            foreach (var c in columns)
            {
                if (c.ColumnOrderingExpression != null)
                {
                    queryable = queryable.OrderBy(
                        c.ColumnOrderingExpression,
                        c.OrderingDirection,
                        alreadyOrdered);
                }
                else
                {
                    var propertyName = c.ColumnOrderingProperty != null
                        ? c.ColumnOrderingProperty.GetPropertyPath()
                        : c.PrivatePropertyName;

                    queryable = queryable.OrderBy(
                        propertyName,
                        c.OrderingDirection,
                        c.OrderingCaseInsensitive,
                        alreadyOrdered);
                }

                alreadyOrdered = true;
            }

            return queryable;
        }
    }
}
