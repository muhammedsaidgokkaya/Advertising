using System.Text.Json.Serialization;

namespace AdminPanel.Models.Organization.User
{
    public class UpdateUser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("orgAddress")]
        public string OrgAddress { get; set; }

        [JsonPropertyName("zipCode")]
        public string ZipCode { get; set; }

        [JsonPropertyName("taskNumber")]
        public string TaskNumber { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("mail")]
        public string Mail { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }
    }
}
