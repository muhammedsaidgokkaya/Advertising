﻿using AdminTest.Models.Meta.Insight;

namespace AdminTest.Models.Meta.Campaign
{
    public class Campaign
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string AccountId { get; set; }

        public InsightResponse Insights { get; set; }

        public DateTime? EndTime { get; set; }
    }
}