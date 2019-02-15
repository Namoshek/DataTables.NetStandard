using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DataTables.NetCore.Util
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// <see cref="MethodInfo"/> for the <see cref="Object.ToString()"/> method.
        /// </summary>
        internal static readonly MethodInfo Object_ToString = typeof(object).GetMethod(nameof(object.ToString));

        /// <summary>
        /// <see cref="MethodInfo"/> for the <see cref="string.ToLower()"/> method.
        /// </summary>
        internal static readonly MethodInfo String_ToLower = typeof(string).GetMethod(nameof(String.ToLower), new Type[] { });

        /// <summary>
        /// <see cref="MethodInfo"/> for the <see cref="string.Contains(char)"/> method.
        /// </summary>
        internal static readonly MethodInfo String_Contains = typeof(string).GetMethod(nameof(String.Contains), new[] { typeof(string) });

        /// <summary>
        /// <see cref="MethodInfo"/> for the <see cref="Regex.IsMatch(string, string)"/> method.
        /// </summary>
        internal static readonly MethodInfo Regex_IsMatch = typeof(Regex).GetMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string) });

        /// <summary>
        /// Builds a parameter expression for the given type
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        internal static ParameterExpression BuildParameterExpression<TEntity>()
        {
            var type = typeof(TEntity);

            return Expression.Parameter(type, "e");
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> for the given <paramref name="propertyName"/>.
        /// The property name has to be given as dot-separated property path, e.g. <code>Destination.Location.City</code>.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="propertyName">Name of the property.</param>
        internal static MemberExpression BuildPropertyExpression(ParameterExpression param, string propertyName)
        {
            string[] parts = propertyName.Split('.');
            Expression body = param;

            foreach (var member in parts)
            {
                body = Expression.Property(body, member);
            }

            return (MemberExpression)body;
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> for the given property name which performs a <see cref="string.Contains(char)"/>
        /// check on the property with the given <paramref name="stringConstant"/> as value.
        /// If required, the method can build an expression using case insensitive string comparison.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="stringConstant">The string constant.</param>
        /// <param name="caseInsensitive">if set to <c>true</c> [case insensitive].</param>
        internal static Expression<Func<TEntity, bool>> BuildStringContainsPredicate<TEntity>(string propertyName, string stringConstant, bool caseInsensitive)
        {
            var parameterExp = BuildParameterExpression<TEntity>();
            var propertyExp = BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            if (propertyExp.Type != typeof(string))
            {
                exp = Expression.Call(propertyExp, Object_ToString);
            }
            
            if (caseInsensitive)
            {
                exp = Expression.Call(exp, String_ToLower);
                stringConstant = stringConstant.ToLower();
            }

            var someValue = Expression.Constant(stringConstant, typeof(string));
            var containsMethodExp = Expression.Call(exp, String_Contains, someValue);

            var notNullExp = Expression.NotEqual(exp, Expression.Constant(null, typeof(object)));

            return Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(notNullExp, containsMethodExp), parameterExp);
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> for the given property name which performs a <see cref="Regex.IsMatch(string, string)"/>
        /// check on the property with the given <paramref name="regex"/> as value.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="regex">The regex.</param>
        internal static Expression<Func<TEntity, bool>> BuildRegexPredicate<TEntity>(string propertyName, string regex)
        {
            var parameterExp = BuildParameterExpression<TEntity>();
            var propertyExp = BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            if (propertyExp.Type != typeof(string))
            {
                exp = Expression.Call(propertyExp, Object_ToString);
            }

            var regexExp = Expression.Constant(regex, typeof(string));
            var resultExp = Expression.Call(Regex_IsMatch, exp, regexExp);

            var notNullExp = Expression.NotEqual(exp, Expression.Constant(null, typeof(object)));

            return Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(notNullExp, resultExp), parameterExp);
        }

        /// <summary>
        /// Replaces the given <paramref name="source"/> with the given <paramref name="target"/>
        /// in the given <paramref name="expression"/>. Can be used to replace lambda variabels with
        /// backed-in constants.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="expression"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        internal static Expression<TOutput> ReplaceVariableWithExpression<TInput, TOutput>(Expression<TInput> expression, ParameterExpression source, Expression target)
        {
            return new ParameterReplacerVisitor<TOutput>(source, target).VisitAndConvert(expression);
        }

        /// <summary>
        /// Utility class that can be used to replace parameters in expressions.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        private class ParameterReplacerVisitor<TOutput> : ExpressionVisitor
        {
            private ParameterExpression _source;
            private Expression _target;

            public ParameterReplacerVisitor(ParameterExpression source, Expression target)
            {
                _source = source;
                _target = target;
            }

            /// <summary>
            /// Replaces the stored source with the stored target in the given <paramref name="root"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="root"></param>
            /// <returns></returns>
            internal Expression<TOutput> VisitAndConvert<T>(Expression<T> root)
            {
                return (Expression<TOutput>)VisitLambda(root);
            }

            /// <summary>
            /// Creates a new expression of a lambda that has the given parameter (source)
            /// replaced with the given expression (target).
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                // Here we select the new list of parameters, without the parameter
                // we want to eliminate
                var parameters = node.Parameters.Where(p => p != _source);

                return Expression.Lambda<TOutput>(Visit(node.Body), parameters);
            }

            /// <summary>
            /// Replace the given parameter (source) with the given expression (target)
            /// by visiting all nodes.
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                // Replace the source with the target, visit other params as usual
                return node == _source ? _target : base.VisitParameter(node);
            }
        }
    }
}
