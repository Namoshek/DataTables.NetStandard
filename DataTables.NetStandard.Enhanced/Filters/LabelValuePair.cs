using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class LabelValuePair
    {
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        public LabelValuePair(string labelAndValue)
        {
            Label = labelAndValue;
            Value = labelAndValue;
        }

        public LabelValuePair(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}
