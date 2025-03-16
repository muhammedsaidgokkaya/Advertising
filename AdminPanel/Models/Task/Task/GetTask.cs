namespace AdminPanel.Models.Task.Task
{
	public class GetTask
	{
		public int Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public string Name { get; set; }
		public int State { get; set; }
		public string CreatedUser { get; set; }
		public DateTime Duration { get; set; }
		public string Content { get; set; }
        public string Department { get; set; }
    }
}
