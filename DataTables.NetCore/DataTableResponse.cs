using DataTables.Queryable;
using Newtonsoft.Json;

namespace DataTables.NetCore
{
    public class DataTableResponse<TEntity>
    {
        [JsonProperty(PropertyName = "draw")]
        public long Draw { get; }

        [JsonProperty(PropertyName = "recordsTotal")]
        public long TotalRecords { get => Data.TotalCount; }

        [JsonProperty(PropertyName = "recordsFiltered")]
        public long FilteredRecords { get => Data.TotalCount; }

        [JsonProperty(PropertyName = "data")]
        public IPagedList<TEntity> Data { get; }

        public DataTableResponse(IPagedList<TEntity> data, long draw = 0)
        {
            Data = data;
            Draw = draw;
        }

        public string AsJsonString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { ContractResolver = DataTableSerializationContractResolver.Instance });
        }
    }
}
