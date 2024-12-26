using System.Text.Json.Serialization;

namespace AdminPanel.Models.Organization.User
{
    public class Password
    {
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; }
    }
}
