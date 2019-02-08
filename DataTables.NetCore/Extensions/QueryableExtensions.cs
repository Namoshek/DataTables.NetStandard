using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataTables.NetCore.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> ApplyFilters<TEntity>(this IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
        {
            return filter.Invoke(query);
        }

        public static IPagedList<TEntity> ToPagedList<TEntity>(this IEnumerable<TEntity> source, DataTablesRequest<TEntity> request)
        {
            return source.AsQueryable().ToPagedList(request);
        }

        public static IPagedList<TEntity> ToPagedList<TEntity>(this IQueryable<TEntity> queryable, DataTablesRequest<TEntity> request)
        {
            return queryable.Filter(request).ToPagedList();
        }

        
        public static IPagedList<TEntity> ToPagedList<TEntity>(this IDataTablesQueryable<TEntity> queryable)
        {
            return new PagedList<TEntity>(queryable);
        }

        public static Task<IPagedList<TEntity>> ToPagedListAsync<TEntity>(this IEnumerable<TEntity> source, DataTablesRequest<TEntity> request)
        {
            return Task.Factory.StartNew(() => source.AsQueryable().ToPagedList(request));
        }

        public static Task<IPagedList<TEntity>> ToPagedListAsync<TEntity>(this IQueryable<TEntity> queryable, DataTablesRequest<TEntity> request)
        {
            return Task.Factory.StartNew(() => queryable.Filter(request).ToPagedList());
        }

        public static Task<IPagedList<TEntity>> ToPagedListAsync<TEntity>(this IDataTablesQueryable<TEntity> queryable)
        {
            return Task.Factory.StartNew<IPagedList<TEntity>>(() => new PagedList<TEntity>(queryable));
        }

        public static IPagedList<TEntity> Apply<TEntity>(this IPagedList<TEntity> list, Action<TEntity> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
            return list;
        }

        public static IPagedList<TResult> Convert<TEntity, TResult>(this IPagedList<TEntity> source, Func<TEntity, TResult> converter)
        {
            var list = new PagedList<TResult>(source);
            foreach (var item in source)
            {
                list.Add(converter(item));
            }
            return list;
        }

        public static IDataTablesQueryable<TEntity> Filter<TEntity>(this IQueryable<TEntity> queryable, DataTablesRequest<TEntity> request)
        {
            // Modify the IQueryable<T> with consecutive steps.
            // If you need to change the order or add extra steps,
            // you should to write own Filter<T> extension method similarly.
            queryable =

                // convert IQueryable<T> to IDataTablesQueryable<T>
                queryable.AsDataTablesQueryable(request)

                // apply custom filter, if specified
                .CustomFilter()

                // perform global search by all searchable columns
                .GlobalSearch()

                // perform individual columns search by all searchable columns
                .ColumnsSearch()

                // order the IDataTablesQueryable<T> by columns listed in the request
                .Order();

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

                request.Log.BeginInvoke(sb.ToString(), null, null);
            }

            return (IDataTablesQueryable<TEntity>)queryable;
        }

        public static IDataTablesQueryable<TEntity> AsDataTablesQueryable<TEntity>(this IQueryable<TEntity> queryable, DataTablesRequest<TEntity> request)
        {
            return new DataTablesQueryable<TEntity>(queryable, request);
        }

        public static IDataTablesQueryable<TEntity> CustomFilter<TEntity>(this IDataTablesQueryable<TEntity> queryable)
        {
            if (queryable.Request.CustomFilterPredicate != null)
            {
                queryable = (IDataTablesQueryable<TEntity>)queryable.Where(queryable.Request.CustomFilterPredicate);
            }
            return queryable;
        }

        public static IDataTablesQueryable<TEntity> GlobalSearch<TEntity>(this IDataTablesQueryable<TEntity> queryable)
        {
            if (!string.IsNullOrEmpty(queryable.Request.GlobalSearchValue))
            {
                // searchable columns
                var columns = queryable.Request.Columns.Where(c => c.IsSearchable);

                if (columns.Any())
                {
                    Expression<Func<TEntity, bool>> predicate = null;
                    foreach (var c in columns)
                    {
                        var expr = c.GlobalSearchPredicate ?? BuildStringContainsPredicate<TEntity>(c.PropertyName, queryable.Request.GlobalSearchValue, c.SearchCaseInsensitive);
                        predicate = predicate == null ?
                            PredicateBuilder.Create(expr) :
                            predicate.Or(expr);
                    }
                    queryable = (IDataTablesQueryable<TEntity>)queryable.Where(predicate);
                }
            }
            return queryable;
        }

        public static IDataTablesQueryable<TEntity> ColumnsSearch<TEntity>(this IDataTablesQueryable<TEntity> queryable)
        {
            // searchable columns
            var columns = queryable.Request.Columns.Where(c =>
                c.IsSearchable &&
                !string.IsNullOrEmpty(c.SearchValue));

            if (columns.Any())
            {
                Expression<Func<TEntity, bool>> predicate = null;
                foreach (var c in columns)
                {
                    var expr = c.ColumnSearchPredicate ?? BuildStringContainsPredicate<TEntity>(c.PropertyName, c.SearchValue, c.SearchCaseInsensitive);
                    predicate = predicate == null ?
                        PredicateBuilder.Create(expr) :
                        predicate.And(expr);
                }
                queryable = (IDataTablesQueryable<TEntity>)queryable.Where(predicate);
            }
            return queryable;
        }

        public static IDataTablesQueryable<TEntity> Order<TEntity>(this IDataTablesQueryable<TEntity> queryable)
        {
            // orderable columns
            var columns = queryable.Request.Columns.Where(c =>
                c.IsOrderable &&
                c.OrderingIndex != -1)
                .OrderBy(c => c.OrderingIndex);

            bool alreadyOrdered = false;

            foreach (var c in columns)
            {
                var propertyName = c.ColumnOrderingProperty != null ? c.ColumnOrderingProperty.GetPropertyPath() : c.PropertyName;
                queryable = (IDataTablesQueryable<TEntity>)queryable.OrderBy(propertyName, c.OrderingDirection, c.OrderingCaseInsensitive, alreadyOrdered);
                alreadyOrdered = true;
            }

            return queryable;
        }

        private static readonly MethodInfo Object_ToString = typeof(object).GetMethod(nameof(object.ToString));

        private static readonly MethodInfo String_ToLower = typeof(string).GetMethod(nameof(String.ToLower), new Type[] { });

        private static readonly MethodInfo String_Contains = typeof(string).GetMethod(nameof(String.Contains), new[] { typeof(string) });

        private static MemberExpression BuildPropertyExpression(ParameterExpression param, string propertyName)
        {
            string[] parts = propertyName.Split('.');
            Expression body = param;
            foreach (var member in parts)
            {
                body = Expression.Property(body, member);
            }
            return (MemberExpression)body;
        }

        private static Expression<Func<TEntity, bool>> BuildStringContainsPredicate<TEntity>(string propertyName, string stringConstant, bool caseInsensitive)
        {
            var type = typeof(TEntity);
            var parameterExp = Expression.Parameter(type, "e");
            var propertyExp = BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            // if the property value type is not string, it needs to be casted at first
            if (propertyExp.Type != typeof(string))
            {
                exp = Expression.Call(propertyExp, Object_ToString);
            }

            var someValue = Expression.Constant(caseInsensitive ? stringConstant.ToLower() : stringConstant, typeof(string));
            var containsMethodExp = Expression.Call(exp, String_Contains, someValue);
            var notNullExp = Expression.NotEqual(exp, Expression.Constant(null, typeof(object)));

            // call ToLower if case insensitive search
            if (caseInsensitive)
            {
                var toLowerExp = Expression.Call(exp, String_ToLower);
                containsMethodExp = Expression.Call(toLowerExp, String_Contains, someValue);
            }

            return Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(notNullExp, containsMethodExp), parameterExp);
        }

        private static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, string propertyName, ListSortDirection direction, bool caseInsensitive, bool alreadyOrdered)
        {
            string methodName = null;

            if (direction == ListSortDirection.Ascending && !alreadyOrdered)
                methodName = nameof(System.Linq.Queryable.OrderBy);
            else if (direction == ListSortDirection.Descending && !alreadyOrdered)
                methodName = nameof(System.Linq.Queryable.OrderByDescending);
            if (direction == ListSortDirection.Ascending && alreadyOrdered)
                methodName = nameof(System.Linq.Queryable.ThenBy);
            else if (direction == ListSortDirection.Descending && alreadyOrdered)
                methodName = nameof(System.Linq.Queryable.ThenByDescending);

            var type = typeof(TEntity);
            var parameterExp = Expression.Parameter(type, "e");
            var propertyExp = BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            // call ToLower if case insensitive search
            if (caseInsensitive && propertyExp.Type == typeof(string))
            {
                exp = Expression.Call(exp, String_ToLower);
            }

            var orderByExp = Expression.Lambda(exp, parameterExp);
            var typeArguments = new Type[] { type, propertyExp.Type };

            var resultExpr = Expression.Call(typeof(System.Linq.Queryable), methodName, typeArguments, query.Expression, Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<TEntity>(resultExpr);
        }
    }
}
