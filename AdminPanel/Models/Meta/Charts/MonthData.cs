using Newtonsoft.Json;

namespace AdminPanel.Models.Meta.Charts
{
    public class MonthData
    {
        [JsonProperty("data")]
        public List<DataItem> Data { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }
    }
}
