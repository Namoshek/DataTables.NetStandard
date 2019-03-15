using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced
{
    public class EnhancedDataTablesResponse<TEntity, TEntityViewModel> : DataTablesResponse<TEntity, TEntityViewModel>
    {
        [JsonExtensionData]
        public IDictionary<string, dynamic> AdditionalData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedDataTablesResponse{TEntity, TEntityViewModel}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="draw">The draw.</param>
        public EnhancedDataTablesResponse(IPagedList<TEntityViewModel> data, 
            IList<DataTablesColumn<TEntity, TEntityViewModel>> columns, 
            long draw = 0,
            IDictionary<string, dynamic> additionalData = null)
            : base(data, columns, draw)
        {
            AdditionalData = additionalData.Where(e => e.Value != null).ToDictionary(e => e.Key, e => e.Value);
        }
    }
}
