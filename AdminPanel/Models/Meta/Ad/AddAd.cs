namespace AdminPanel.Models.Meta.Ad
{
    public class AddAd
    {
        public class AdDto
        {
            public string AdName { get; set; }
            public string AdAccountId { get; set; }
            public string AdSetId { get; set; }
            public string FacebookPageId { get; set; }
            public string InstagramId { get; set; }
            public string WebsiteUrl { get; set; }
            public string MainText { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string CallToActionType { get; set; }
            public string Objective { get; set; }
            public IFormFile Image { get; set; }
        }

        public class AdMultiDto
        {
            public string AdName { get; set; }
            public string AdAccountId { get; set; }
            public string AdSetId { get; set; }
            public string FacebookPageId { get; set; }
            public string InstagramId { get; set; }
            public string WebsiteUrl { get; set; }
            public string MainText { get; set; }
            public string CallToActionType { get; set; }
            public string Objective { get; set; }
            public List<Slide> Slide {  get; set; }
        }

        public class Slide
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string WebsiteUrl { get; set; }
            public IFormFile Image { get; set; }
        }

        public class MetaImageUploadResponse
        {
            public Dictionary<string, MetaImageData> images { get; set; }
        }

        public class MetaImageData
        {
            public string hash { get; set; }
            public string url { get; set; }
        }

        public class MetaCreativeResponse
        {
            public long Id { get; set; }
        }
    }
}
