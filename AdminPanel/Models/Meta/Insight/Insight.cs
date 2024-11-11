using Newtonsoft.Json;

namespace AdminPanel.Models.Meta.Insight
{
    public class Insight
    {
        public int Reach { get; set; }

        public int Impressions { get; set; }

        public double Cpc { get; set; }

        public double Cpm { get; set; }

        public double Spend { get; set; }

        public string QualityRanking { get; set; }

        public string EngagementRateRanking { get; set; }

        public string ConversionRateRanking { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateStop { get; set; }

        public List<Action.Action> Actions { get; set; }
    }
}
