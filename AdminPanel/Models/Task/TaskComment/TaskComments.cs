namespace AdminPanel.Models.Task.TaskComment
{
	public class TaskComments
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime PostedAt { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
    }
}
