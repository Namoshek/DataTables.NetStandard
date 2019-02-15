using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataTables.NetCore.Util;

namespace DataTables.NetCore.Extensions
{
    public static class DataTablesQueryableExtensions
    {
        /// <summary>
        /// Builds a <see cref="PagedList{TEntity, TEntityViewModel}"/> from the given <see cref="IDataTablesQueryable{TEntity, TEntityViewModel}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        public static IPagedList<TEntityViewModel> ToPagedList<TEntity, TEntityViewModel>(this IDataTablesQueryable<TEntity, TEntityViewModel> queryable)
        {
            return new PagedList<TEntity, TEntityViewModel>(queryable);
        }

        /// <summary>
        /// Builds a <see cref="PagedList{TEntity, TEntityViewModel}"/> from the given <see cref="IDataTablesQueryable{TEntity, TEntityViewModel}"/> asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <returns></returns>
        public static Task<IPagedList<TEntityViewModel>> ToPagedListAsync<TEntity, TEntityViewModel>(this IDataTablesQueryable<TEntity, TEntityViewModel> queryable)
        {
            return Task.Factory.StartNew<IPagedList<TEntityViewModel>>(() => new PagedList<TEntity, TEntityViewModel>(queryable));
        }

        /// <summary>
        /// Applies the <see cref="DataTablesRequest{TEntity, TEntityViewModel}.GlobalSearchValue"/> of the given
        /// <see cref="IDataTablesQueryable{TEntity, TEntityViewModel}"/> to the query, if set.
        /// To perform the global search, the method will add a search expression for each searchable column of the request.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        public static IDataTablesQueryable<TEntity, TEntityViewModel> ApplyGlobalSearchFilter<TEntity, TEntityViewModel>(
            this IDataTablesQueryable<TEntity, TEntityViewModel> queryable)
        {
            var globalSearchValue = queryable.Request.GlobalSearchValue;
            if (!string.IsNullOrEmpty(globalSearchValue))
            {
                var columns = queryable.Request.Columns.Where(c => c.IsSearchable);

                if (columns.Any())
                {
                    Expression<Func<TEntity, bool>> predicate = null;

                    foreach (var c in columns)
                    {
                        Expression<Func<TEntity, bool>> expression;

                        if (c.GlobalSearchPredicate != null)
                        {
                            var expr = c.GlobalSearchPredicate;
                            var source = expr.Parameters.Single(p => p.Type == typeof(string));
                            var target = Expression.Constant(globalSearchValue);
                            expression = ExpressionHelper.ReplaceVariableWithExpression<Func<TEntity, string, bool>, Func<TEntity, bool>>(expr, source, target);
                        }
                        else
                        {
                            if (queryable.Request.GlobalSearchRegex)
                            {
                                expression = ExpressionHelper.BuildRegexPredicate<TEntity>(c.PrivatePropertyName, queryable.Request.GlobalSearchValue);
                            }
                            else
                            {
                                expression = ExpressionHelper.BuildStringContainsPredicate<TEntity>(c.PrivatePropertyName,
                                    queryable.Request.GlobalSearchValue, c.SearchCaseInsensitive);
                            }
                        }

                        predicate = predicate == null
                            ? PredicateBuilder.Create(expression)
                            : predicate.Or(expression);
                    }

                    queryable = (IDataTablesQueryable<TEntity, TEntityViewModel>)queryable.Where(predicate);
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
        /// <param name="queryable">The queryable.</param>
        public static IDataTablesQueryable<TEntity, TEntityViewModel> ApplyColumnSearchFilter<TEntity, TEntityViewModel>(
            this IDataTablesQueryable<TEntity, TEntityViewModel> queryable)
        {
            var columns = queryable.Request.Columns
                .Where(c => c.IsSearchable && !string.IsNullOrEmpty(c.SearchValue));

            if (columns.Any())
            {
                Expression<Func<TEntity, bool>> predicate = null;

                foreach (var c in columns)
                {
                    Expression<Func<TEntity, bool>> expression;

                    if (c.ColumnSearchPredicate != null)
                    {
                        var expr = c.ColumnSearchPredicate;
                        var source = expr.Parameters.Single(p => p.Type == typeof(string));
                        var target = Expression.Constant(c.SearchValue);
                        expression = ExpressionHelper.ReplaceVariableWithExpression<Func<TEntity, string, bool>, Func<TEntity, bool>>(expr, source, target);
                    }
                    else
                    {
                        if (c.SearchRegex)
                        {
                            expression = ExpressionHelper.BuildRegexPredicate<TEntity>(c.PrivatePropertyName, c.SearchValue);
                        }
                        else
                        {
                            expression = ExpressionHelper.BuildStringContainsPredicate<TEntity>(c.PrivatePropertyName,
                                c.SearchValue, c.SearchCaseInsensitive);
                        }
                    }

                    predicate = predicate == null
                        ? PredicateBuilder.Create(expression)
                        : predicate.And(expression);
                }

                queryable = (IDataTablesQueryable<TEntity, TEntityViewModel>)queryable.Where(predicate);
            }

            return queryable;
        }

        /// <summary>
        /// Applies the requested order for each of the <see cref="DataTablesRequest{TEntity, TEntityViewModel}.Columns"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <returns></returns>
        public static IDataTablesQueryable<TEntity, TEntityViewModel> ApplyOrder<TEntity, TEntityViewModel>(this IDataTablesQueryable<TEntity, TEntityViewModel> queryable)
        {
            var columns = queryable.Request.Columns
                .Where(c => c.IsOrderable && c.OrderingIndex >= 0)
                .OrderBy(c => c.OrderingIndex);

            bool alreadyOrdered = false;

            foreach (var c in columns)
            {
                var propertyName = c.ColumnOrderingProperty != null
                    ? c.ColumnOrderingProperty.GetPropertyPath() 
                    : c.PrivatePropertyName;

                queryable = (IDataTablesQueryable<TEntity, TEntityViewModel>)queryable.OrderBy(propertyName, c.OrderingDirection, c.OrderingCaseInsensitive, alreadyOrdered);

                alreadyOrdered = true;
            }

            return queryable;
        }
    }
}
