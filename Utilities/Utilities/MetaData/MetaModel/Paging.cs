using Newtonsoft.Json;

namespace Utilities.Utilities.MetaData.MetaModel
{
    public class Paging
    {
        [JsonProperty("cursors")]
        public Cursors Cursors { get; set; }
    }
}
