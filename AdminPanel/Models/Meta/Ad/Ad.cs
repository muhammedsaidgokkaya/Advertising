using AdminPanel.Models.Meta.Insight;
using Newtonsoft.Json;

namespace AdminPanel.Models.Meta.Ad
{
    public class Ad
    {
        public string Name { get; set; }

        public string Status { get; set; }

        public AdSet.AdSet AdSet { get; set; }

        public InsightResponse Insights { get; set; }
    }
}
