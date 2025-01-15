using Newtonsoft.Json;

namespace Utilities.Utilities.MetaData.MetaModel
{
    public class DataItem
    {
        [JsonProperty("clicks")]
        public string Clicks { get; set; }

        [JsonProperty("impressions")]
        public string Impressions { get; set; }

        [JsonProperty("spend")]
        public string Spend { get; set; }

        [JsonProperty("frequency")]
        public string Frequency { get; set; }

        [JsonProperty("reach")]
        public string Reach { get; set; }

        [JsonProperty("date_start")]
        public string DateStart { get; set; }

        [JsonProperty("date_stop")]
        public string DateStop { get; set; }
    }
}
