namespace AdminPanel.Models.Google.Analytics.GeneralQuery
{
    public class GeneralCountResponse
    {
        public string Dimension { get; set; }

        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int NewUsers { get; set; }

        public int ScreenPageViews { get; set; }

        public int Sessions { get; set; }

        public int EventCount { get; set; }

        public int KeyEvents { get; set; }

        public double TotalRevenue { get; set; }

        public int Transactions { get; set; }
    }
}
