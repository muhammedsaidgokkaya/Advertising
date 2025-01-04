using Newtonsoft.Json;

namespace AdminPanel.Models.Meta.Charts
{
    public class Paging
    {
        [JsonProperty("cursors")]
        public Cursors Cursors { get; set; }
    }
}
