namespace AdminPanel.Models.Task.Task
{
	public class Tasks
	{
        public int Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public string Name { get; set; }
        public int State { get; set; }
		public int Priority { get; set; }
		public string CreatedUser { get; set; }
        public DateTime Duration { get; set; }
        public int Team { get; set; }
    }
}
