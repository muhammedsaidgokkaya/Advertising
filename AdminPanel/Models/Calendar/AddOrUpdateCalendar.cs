namespace AdminPanel.Models.Calendar
{
	public class AddOrUpdateCalendar
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Color { get; set; }
		public string? Mail { get; set; }
		public string? Phone { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
        public bool IsConfirmation { get; set; }
        public bool AllDay { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}
