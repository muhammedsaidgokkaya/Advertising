using System.Text.Json.Serialization;

namespace AdminPanel.Models.Calendar
{
	public class AddSchemaCalendar
	{
		[JsonPropertyName("name")]
		public List<string> Name { get; set; }
	}
}
