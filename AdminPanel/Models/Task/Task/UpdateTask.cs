namespace AdminPanel.Models.Task.Task
{
	public class UpdateTask
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime Durations { get; set; }
        public string[] Departments { get; set; }
        public List<UserDto> Users { get; set; }
        public List<ServiceDto> Services { get; set; }
	}
}
