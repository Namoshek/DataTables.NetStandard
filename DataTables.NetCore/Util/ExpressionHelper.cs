using System;
using System.Linq.Expressions;
using System.Reflection;

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
            var type = typeof(TEntity);
            var parameterExp = Expression.Parameter(type, "e");
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
    }
}
