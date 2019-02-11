using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataTables.NetCore
{
    /// <summary>
    /// Represents a response containing DataTablesRequest results.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
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

        protected IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTablesResponse{TEntity, TEntityViewModel}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="draw">The draw.</param>
        public DataTablesResponse(IPagedList<TEntityViewModel> data, IList<DataTablesColumn<TEntity, TEntityViewModel>> columns, long draw = 0)
        {
            Data = data;
            Columns = columns;
            Draw = draw;
        }

        /// <summary>
        /// Serializes the whole entity as JSON string. Uses the column configurations to properly
        /// format the result and its keys.
        /// </summary>
        /// <returns></returns>
        public string AsJsonString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                ContractResolver = new DataTablesSerializationContractResolver<TEntity, TEntityViewModel>(Columns)
            });
        }
    }
}
