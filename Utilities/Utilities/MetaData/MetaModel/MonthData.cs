using Newtonsoft.Json;

namespace Utilities.Utilities.MetaData.MetaModel
{
    public class MonthData
    {
        [JsonProperty("data")]
        public List<DataItem> Data { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }
    }
}
