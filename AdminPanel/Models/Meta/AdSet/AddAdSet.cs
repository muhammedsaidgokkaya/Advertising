namespace AdminPanel.Models.Meta.AdSet
{
    public class AddAdSet
    {
        public class AdSetDto
        {
            public string AdSetName { get; set; }
            public string BidFinished { get; set; }
            public string BidStrategy { get; set; }
            public string Budget { get; set; }
            public string Daily { get; set; }
            public DateTime? EndDate { get; set; }
            public List<string> FacebookPositions { get; set; }
            public List<string> InstagramPositions { get; set; }
            public List<string> PublisherPlatforms { get; set; }
            public List<string> MessengerPositions { get; set; }
            public List<string> AudienceNetworkPositions { get; set; }
            public string SelectedAudienceId { get; set; }
            public string SelectedAudienceType { get; set; }
            public string SelectedAccountId { get; set; }
            public string SelectedCampaignId { get; set; }
            public string SelectedFacebookPageId { get; set; }
            public string SelectedInstagramAccountId { get; set; }
            public string SelectedCampaignType { get; set; }
            public string SelectedPixelId { get; set; }
            public string SelectedCampaignObjectiveType { get; set; }
            public string ConversionEvent { get; set; }
            public string? BillingEvent { get; set; }
            public bool IsAdvantage { get; set; }
            public DateTime StartDate { get; set; }
        }
    }
}
