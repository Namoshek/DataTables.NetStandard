using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataTables.NetStandard
{
    /// <summary>
    /// Enables the efficient, dynamic composition of query predicates.
    /// Original source code is taken from: 
    /// http://www.albahari.com/nutshell/predicatebuilder.aspx
    /// </summary>
    internal static class PredicateBuilder
    {
        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        /// <summary>
        /// Combines the first expression with the second using the specified merge function.
        /// </summary>
        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // Zip parameters (map from parameters of second expression to parameters of first expression).
            var map = first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);

            // Replace parameters in the second lambda expression with the parameters from the first expression.
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // Create a merged lambda expression with parameters from the first expression.
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        /// A helper to replace <see cref="ParameterExpression"/> in an <see cref="Expression"/>.
        /// </summary>
        private class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> map;

            private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            /// <summary>
            /// Replace expressions in <paramref name="exp"/> based on the <paramref name="map"/>.
            /// </summary>
            /// <param name="map"></param>
            /// <param name="exp"></param>
            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            /// <inheritdoc/>
            protected override Expression VisitParameter(ParameterExpression p)
            {
                if (map.TryGetValue(p, out ParameterExpression replacement))
                {
                    p = replacement;
                }

                return base.VisitParameter(p);
            }
        }
    }
}
