using System.Reflection;
using DataTables.NetCore.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataTables.NetCore
{
    internal class DataTableSerializationContractResolver : DefaultContractResolver
    {
        public static readonly DataTableSerializationContractResolver Instance = new DataTableSerializationContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var columnAttribute = member.GetCustomAttribute<DTColumn>();
            if (columnAttribute != null)
            {
                property.PropertyName = columnAttribute.Data;
            }

            return property;
        }
    }
}
