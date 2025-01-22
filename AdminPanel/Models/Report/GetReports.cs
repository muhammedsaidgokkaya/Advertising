namespace AdminPanel.Models.Report
{
    public class GetReports
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string TypeId { get; set; }
        public DateTime? InsertedDate { get; set; }
    }
}
