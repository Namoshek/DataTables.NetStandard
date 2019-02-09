using Newtonsoft.Json;

namespace DataTables.NetCore
{
    public class DataTablesResponse<TEntity, TEntityViewModel>
    {
        [JsonProperty(PropertyName = "draw")]
        public long Draw { get; }

        [JsonProperty(PropertyName = "recordsTotal")]
        public long TotalRecords { get => Data.TotalCount; }

        [JsonProperty(PropertyName = "recordsFiltered")]
        public long FilteredRecords { get => Data.TotalCount; }

        [JsonProperty(PropertyName = "data")]
        public IPagedList<TEntityViewModel> Data { get; }

        protected IDataTablesColumnsCollection<TEntity, TEntityViewModel> Columns { get; set; }

        public DataTablesResponse(IPagedList<TEntityViewModel> data, IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns, long draw = 0)
        {
            Data = data;
            Columns = columns;
            Draw = draw;
        }

        public string AsJsonString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                ContractResolver = new DataTablesSerializationContractResolver<TEntity, TEntityViewModel>(Columns)
            });
        }
    }
}
