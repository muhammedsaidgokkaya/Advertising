using System.Text.Json.Serialization;

namespace AdminPanel.Models.Meta.Audience
{
    public class CustomAudienceRequest
    {
        [JsonPropertyName("selectedCountries")]
        public string[] SelectedCountries { get; set; }

        [JsonPropertyName("ratio")]
        public int[] Ratios { get; set; }

        [JsonPropertyName("selectedAudience")]
        public string SelectedAudience { get; set; }

        [JsonPropertyName("selectedAccount")]
        public string SelectedAccount { get; set; }
    }
}
