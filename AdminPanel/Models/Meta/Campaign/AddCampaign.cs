namespace AdminPanel.Models.Meta.Campaign
{
    public class AddCampaign
    {
        public string CampaignName { get; set; }
        public string SelectedAccountId { get; set; }
        public string SelectedPriceType { get; set; }
        public string SelectedType { get; set; }
        public string Daily { get; set; }
        public string Budget { get; set; }
        public string BidStrategy { get; set; }
        public bool AdvantageBudget { get; set; }
    }
}
