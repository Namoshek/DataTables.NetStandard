using System.Linq.Expressions;
using System.Text;

namespace DataTables.NetStandard.Extensions
{
    /// <summary>
    /// Utility extension methods for <see cref="Expression" />.
    /// </summary>
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the property path from an expression.
        /// </summary>
        /// <param name="expr">Expression instance, for example 'p => p.Office.Street.Address'.</param>
        /// <returns>Full property path like "Office.Street.Address"</returns>
        public static string GetPropertyPath(this Expression expr)
        {
            var path = new StringBuilder();
            var memberExpression = GetMemberExpression(expr);

            do
            {
                if (path.Length > 0)
                {
                    path.Insert(0, ".");
                }
                
                path.Insert(0, memberExpression.Member.Name);

                memberExpression = GetMemberExpression(memberExpression.Expression);
            }
            while (memberExpression != null);

            return path.ToString();
        }

        /// <summary>
        /// Retrieve the nearest <see cref="MemberExpression"/> from the given <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression"></param>
        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression;
            }

            if (expression is LambdaExpression)
            {
                var lambdaExpression = expression as LambdaExpression;
                
                if (lambdaExpression.Body is MemberExpression memberExpression2)
                {
                    return memberExpression2;
                }

                if (lambdaExpression.Body is UnaryExpression unaryExpression)
                {
                    return (MemberExpression)unaryExpression.Operand;
                }
            }

            return null;
        }
    }
}
