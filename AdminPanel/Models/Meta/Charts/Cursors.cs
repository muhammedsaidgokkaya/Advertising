using Newtonsoft.Json;

namespace AdminPanel.Models.Meta.Charts
{
    public class Cursors
    {
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }
    }
}
