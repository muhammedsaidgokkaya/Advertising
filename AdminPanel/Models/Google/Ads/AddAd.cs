namespace AdminPanel.Models.Google.Ads
{
    public class AddAd
    {
        public class SaveRequestModel
        {
            public string SelectedAccountId { get; set; }
            public long SelectedAdGroupId { get; set; }
            public long SelectedCampaignId { get; set; }
            public string SelectedCampaignType { get; set; }
            public List<string> Descriptions { get; set; }
            public List<string> Headlines { get; set; }
            public string AccountName { get; set; }
            public string FinalUrl { get; set; }
            public string AdName { get; set; }
            public string Logo { get; set; }
            public string Url1 { get; set; }
            public string Url2 { get; set; }
        }

        public class SaveDisplayRequestModel
        {
            public List<IFormFile> Images { get; set; }
            public List<IFormFile> Logos { get; set; }
            public IFormFile? Video { get; set; }
            public string SelectedAccountId { get; set; }
            public long SelectedAdGroupId { get; set; }
            public long SelectedCampaignId { get; set; }
            public string SelectedCampaignType { get; set; }
            public List<string> Descriptions { get; set; }
            public List<string> Headlines { get; set; }
            public string AccountName { get; set; }
            public string FinalUrl { get; set; }
            public string AdName { get; set; }
            public string LongTittle { get; set; }
        }
    }
}
