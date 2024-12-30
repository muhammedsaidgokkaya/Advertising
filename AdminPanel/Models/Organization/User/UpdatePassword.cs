using System.Text.Json.Serialization;

namespace AdminPanel.Models.Organization.User
{
    public class UpdatePassword
    {
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; }
    }
}
