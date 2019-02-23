using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class FilterOptions
    {
        [JsonProperty(PropertyName = "column_number")]
        public int ColumnNumber { get; set; }

        [JsonProperty(PropertyName = "filter_delay")]
        public int FilterDelay { get; set; }

        [JsonProperty(PropertyName = "filter_type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "style_class")]
        public string StyleClass { get; set; }

        [JsonIgnore]
        public string FilterResetButtonText { get; set; }

        [JsonProperty(PropertyName = "filter_reset_button_text")]
        public dynamic FilterResetButtonTextSerializer
        {
            get
            {
                if (FilterResetButtonText == null)
                {
                    return false;
                }

                return FilterResetButtonText;
            }
        }

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public IList<object> Data { get; set; }

        [JsonExtensionData]
        public IDictionary<string, dynamic> AdditionalOptions { get; set; } = new Dictionary<string, dynamic>();
    }
}
