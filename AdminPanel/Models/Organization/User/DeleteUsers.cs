using System.Text.Json.Serialization;

namespace AdminPanel.Models.Organization.User
{
    public class DeleteUsers
    {
        [JsonPropertyName("userId")]
        public List<int> UserId { get; set; } = new List<int>();
    }
}
