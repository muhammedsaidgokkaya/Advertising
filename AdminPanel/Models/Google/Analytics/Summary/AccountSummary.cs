namespace AdminPanel.Models.Google.Analytics.Summary
{
    public class AccountSummary
    {
        public string Name { get; set; }

        public string Account { get; set; }

        public string DisplayName { get; set; }

        public List<PropertySummary> PropertySummaries { get; set; }
    }
}
