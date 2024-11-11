using AdminPanel.Models.Meta.Insight;
using Newtonsoft.Json;

namespace AdminPanel.Models.Meta.AdSet
{
    public class AdSet
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string BidStrategy { get; set; }

        public int DailyBudget { get; set; }

        public int LifeTimeBudget { get; set; }

        public DateTime? UpdateTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public InsightResponse Insights { get; set; }
    }
}
