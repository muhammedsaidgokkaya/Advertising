using Newtonsoft.Json;

namespace AdminPanel.Models
{
    public class AdSetViewModel
    {
        public class AdSetResponse
        {
            [JsonProperty("data")]
            public List<AdSet> Data { get; set; }
        }

        public class AdSet
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("bid_strategy")]
            public string BidStrategy { get; set; }

            [JsonProperty("daily_budget")]
            public string DailyBudget { get; set; }

            [JsonProperty("lifetime_budget")]
            public string LifeTimeBudget { get; set; }

            [JsonProperty("updated_time")]
            public string UpdateTime { get; set; }

            [JsonProperty("start_time")]
            public string StartTime { get; set; }

            [JsonProperty("end_time")]
            public string EndTime { get; set; }

            [JsonProperty("insights")]
            public InsightResponse Insights { get; set; }
        }

        public class InsightResponse
        {
            [JsonProperty("data")]
            public List<Insight> Data { get; set; }
        }

        public class Insight
        {
            [JsonProperty("reach")]
            public string Reach { get; set; }

            [JsonProperty("impressions")]
            public string Impressions { get; set; }

            [JsonProperty("cpc")]
            public string Cpc { get; set; }

            [JsonProperty("cpm")]
            public string Cpm { get; set; }

            [JsonProperty("spend")]
            public string Spend { get; set; }

            [JsonProperty("date_start")]
            public string DateStart { get; set; }

            [JsonProperty("date_stop")]
            public string DateStop { get; set; }

            [JsonProperty("actions")]
            public List<Action> Actions { get; set; }
        }

        public class Action
        {
            [JsonProperty("action_type")]
            public string ActionType { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }
        }
    }
}
