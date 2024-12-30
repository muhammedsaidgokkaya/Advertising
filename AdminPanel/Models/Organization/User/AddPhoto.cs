using System.Text.Json.Serialization;

namespace AdminPanel.Models.Organization.User
{
    public class AddPhoto
    {
        [JsonPropertyName("photo")]
        public IFormFile? Photo { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }
    }
}
