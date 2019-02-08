using System;
using System.Linq;
using System.Reflection;

namespace DataTables.NetCore.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets <see cref="PropertyInfo"/> from full property name.
        /// </summary>
        /// <param name="type">Type to get the property from</param>
        /// <param name="propertyName">Full property name. Can contain dots, like "SomeProperty.NestedProperty" to access to nested comlplex types.</param>
        /// <returns><see cref="PropertyInfo"/> instance.</returns>
        internal static PropertyInfo GetPropertyByName(this Type type, string propertyName)
        {
            string[] parts = propertyName.Split('.');
            if (parts.Length > 1)
            {
                var propertyInfo = type.GetProperty(parts[0]);
                if (propertyInfo == null)
                {
                    return null;
                }
                return GetPropertyByName(propertyInfo.PropertyType, parts.Skip(1).Aggregate((a, i) => $"{a}.{i}"));
            }
            else
            {
                return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            }
        }
    }
}
