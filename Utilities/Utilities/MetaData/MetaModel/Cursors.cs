using Newtonsoft.Json;

namespace Utilities.Utilities.MetaData.MetaModel
{
    public class Cursors
    {
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }
    }
}
