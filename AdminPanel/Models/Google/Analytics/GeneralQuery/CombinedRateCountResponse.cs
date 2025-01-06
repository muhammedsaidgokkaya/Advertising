namespace AdminPanel.Models.Google.Analytics.GeneralQuery
{
    public class CombinedRateCountResponse
    {
        public int Id { get; set; }
        public string Dimension { get; set; }
        public double? AverageSessionDuration { get; set; }
        public double? EventsPerSession { get; set; }
        public double? SessionKeyEventRate { get; set; }
        public double? ScreenPageViewsPerSession { get; set; }
        public double? EngagementRate { get; set; }
        public int? EngagedSessions { get; set; }
        public double? ScreenPageViewsPerUser { get; set; }
        public double? EventCountPerUser { get; set; }
        public double? UserKeyEventRate { get; set; }
        public int? TotalUsers { get; set; }
        public int? ActiveUsers { get; set; }
        public int? NewUsers { get; set; }
        public int? ScreenPageViews { get; set; }
        public int? Sessions { get; set; }
        public int? EventCount { get; set; }
        public int? KeyEvents { get; set; }
        public double? TotalRevenue { get; set; }
        public int? Transactions { get; set; }
    }
}
