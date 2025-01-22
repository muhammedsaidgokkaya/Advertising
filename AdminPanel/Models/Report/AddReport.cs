namespace AdminPanel.Models.Report
{
    public class AddReport
    {
        public string Account { get; set; }
        public string AccountId { get; set; }
        public int TypeId { get; set; }
        public string Content { get; set; }
        public int ReportType { get; set; }
        public int OrganizationId { get; set; }
    }
}
