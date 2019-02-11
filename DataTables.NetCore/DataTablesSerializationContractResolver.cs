using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataTables.NetCore
{
    /// <summary>
    /// A custom contract resolver that ensures the serialization results of a <see cref="DataTablesResponse{TEntity, TEntityViewModel}"/>
    /// match the column configurations defined in <see cref="IList{DataTableColumn{TEntity, TEntityViewModel}}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
    /// <seealso cref="DefaultContractResolver" />
    internal class DataTablesSerializationContractResolver<TEntity, TEntityViewModel> : DefaultContractResolver
    {
        protected readonly IList<DataTablesColumn<TEntity, TEntityViewModel>> _columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTablesSerializationContractResolver{TEntity, TEntityViewModel}"/> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        public DataTablesSerializationContractResolver(IList<DataTablesColumn<TEntity, TEntityViewModel>> columns)
        {
            _columns = columns;
        }

        /// <summary>
        /// Creates a <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
        /// </summary>
        /// <param name="member">The member to create a <see cref="JsonProperty" /> for.</param>
        /// <param name="memberSerialization">The member's parent <see cref="MemberSerialization" />.</param>
        /// <returns>
        /// A created <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
        /// </returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var column = _columns.FirstOrDefault(c => c.PublicPropertyName == property.PropertyName);
            if (column != null)
            {
                property.PropertyName = column.PublicName;
            }

            return property;
        }
    }
}
