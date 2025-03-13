using System.Text.Json.Serialization;

namespace AdminPanel.Models.Task.TaskTemplate
{
	public class AddSchema
	{
		[JsonPropertyName("name")]
		public List<string> Name { get; set; }
	}
}
