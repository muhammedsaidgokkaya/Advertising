namespace AdminPanel.Models.Google.Ads
{
    public class AddCampaign
    {
        public class SearchCampaignRequest
        {
            public long SelectedAccountId { get; set; }
            public string SelectedType { get; set; }
            public string CampaignName { get; set; }
            public Results Results { get; set; }
            public string Website { get; set; }
            public string PhoneNumber { get; set; }
            public string BiddingType { get; set; }
            public string TargetCpa { get; set; }
            public string TargetRoas { get; set; }
            public string MaxCpcLimit { get; set; }
            public string ImpressionPosition { get; set; }
            public string ImpressionShareTarget { get; set; }
            public string MaxCpcImpressionLimit { get; set; }
            public string Budget { get; set; }
            public List<int> SelectedLanguages { get; set; }
            public string Locations { get; set; }
            public List<LocationAddress> CustomLocations { get; set; }
        }

        public class Results
        {
            public bool WebsiteVisits { get; set; }
            public bool PhoneCalls { get; set; }
        }

        public class LocationAddress
        {
            public string? Country { get; set; }
            public string? Region { get; set; }
            public string? State { get; set; }
            public string? Town { get; set; }
            public string? country_code { get; set; }
        }
    }
}
