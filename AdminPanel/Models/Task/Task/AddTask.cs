namespace AdminPanel.Models.Task.Task
{
	public class AddTask
	{
		public string Name { get; set; }
		public string Content { get; set; }
		public DateTime Durations { get; set; }
		public List<string> Departments { get; set; }
		public List<int> Users { get; set; }
		public List<int> Services { get; set; }
	}
}
