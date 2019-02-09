using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataTables.NetCore
{
    internal class DataTablesSerializationContractResolver<TEntity, TEntityViewModel> : DefaultContractResolver
    {
        protected readonly IDataTablesColumnsCollection<TEntity, TEntityViewModel> _columns;

        public DataTablesSerializationContractResolver(IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns)
        {
            _columns = columns;
        }

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
