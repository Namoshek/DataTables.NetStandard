using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DataTables.NetStandard.Util
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
        /// Builds a parameter expression for the given type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        internal static ParameterExpression BuildParameterExpression<TEntity>()
        {
            return Expression.Parameter(typeof(TEntity));
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> for the given <paramref name="propertyName"/>.
        /// The property name has to be given as dot-separated property path, e.g. <c>Destination.Location.City</c>.
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
        internal static Expression<Func<TEntity, bool>> BuildStringContainsPredicate<TEntity>(
            string propertyName,
            string stringConstant,
            bool caseInsensitive)
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

            var someValue = CreateConstantFilterExpression(stringConstant, typeof(string));
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

            var regexExp = CreateConstantFilterExpression(regex, typeof(string));
            var resultExp = Expression.Call(Regex_IsMatch, exp, regexExp);

            var notNullExp = Expression.NotEqual(exp, Expression.Constant(null, typeof(object)));

            return Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(notNullExp, resultExp), parameterExp);
        }

        /// <summary>
        /// Creates a constant filter expression of the given <paramref name="value"/> and converts the type to the given <paramref name="type"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        internal static Expression CreateConstantFilterExpression(object value, Type type)
        {
            // The value is converted to anonymous function only returning the value itself.
            Expression<Func<object>> valueExpression = () => value;

            // Afterwards only the body of the function, which is the value, is converted to the delivered type.
            // Therefore no Expression.Constant is necessary which lead to memory leaks, because EFCore caches such constants.
            // Caching constants is not wrong, but creating constants of dynamic search values is wrong.
            return Expression.Convert(valueExpression.Body, type);
        }
    }
}
