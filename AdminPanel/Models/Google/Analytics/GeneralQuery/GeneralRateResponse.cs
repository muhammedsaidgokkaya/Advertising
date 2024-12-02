using Newtonsoft.Json;

namespace AdminPanel.Models.Google.Analytics.GeneralQuery
{
    public class GeneralRateResponse
    {
        public string Dimension { get; set; }

        public double AverageSessionDuration { get; set; }

        public double EventsPerSession { get; set; }

        public double SessionKeyEventRate { get; set; }

        public double ScreenPageViewsPerSession { get; set; }

        public double EngagementRate { get; set; }

        public int EngagedSessions { get; set; }

        public double ScreenPageViewsPerUser { get; set; }

        public double EventCountPerUser { get; set; }

        public double UserKeyEventRate { get; set; }
    }
}
