using Newtonsoft.Json;

namespace AdminPanel.Models
{
    public class CampaignAdminViewModel
    {
        public class CampaignResponse
        {
            [JsonProperty("data")]
            public List<Campaign> Data { get; set; }
        }

        public class Campaign
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("account_id")]
            public string AccountId { get; set; }

            [JsonProperty("insights")]
            public InsightResponse Insights { get; set; }

            [JsonProperty("end_time")]
            public string EndTime { get; set; }
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
