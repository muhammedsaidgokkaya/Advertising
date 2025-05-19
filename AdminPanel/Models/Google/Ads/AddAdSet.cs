namespace AdminPanel.Models.Google.Ads
{
    public class AddAdSet
    {
        public class CampaignSaveRequest
        {
            public string AdGroupName { get; set; }
            public List<string> Chips { get; set; }
            public string SelectedAccountId { get; set; }
            public long SelectedCampaignId { get; set; }
            public string SelectedCampaignType { get; set; }
        }
    }
}
