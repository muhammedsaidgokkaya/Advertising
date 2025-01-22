namespace AdminPanel.Models.Report
{
    public class GetReport
    {
        public string Name { get; set; }
        public string Account { get; set; }
        public string TypeId { get; set; }
        public string Content { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
