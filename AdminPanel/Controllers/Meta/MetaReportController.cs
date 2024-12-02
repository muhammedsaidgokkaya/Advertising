using AdminPanel.Models.Meta.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.MetaData;

namespace AdminPanel.Controllers.Meta
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaReportController : ControllerBase
    {
        private readonly ILogger<MetaReportController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;
        private readonly MetaData _metaData;
        private readonly DefaultValues _defaultValues;

        public MetaReportController(ILogger<MetaReportController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
            _metaData = new MetaData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<ReportFilterResponse>> GetInsightsReport(int userId, string accountId, string reportFilter, DateTime? startDate = null, DateTime? endDate = null)
        {
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var insightsFilter = _metaData.InsightsFilterAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"), reportFilter);
            var data = new ReportFilterResponse
            {
                Data = insightsFilter.Data?.Select(q => new ReportFilter
                {
                    Reach = q.Reach,
                    Frequency = q.Frequency,
                    Ctr = q.Ctr,
                    Impressions = q.Impressions,
                    Cpc = q.Cpc,
                    Cpm = q.Cpm,
                    Spend = q.Spend,
                    Clicks = q.Clicks,
                    DateStart = q.DateStart,
                    DateStop = q.DateStop,
                    AdFormatAsset = q.AdFormatAsset ?? string.Empty,
                    Age = q.Age ?? string.Empty,
                    AppId = q.AppId ?? string.Empty,
                    BodyAsset = q.BodyAsset ?? string.Empty,
                    BreakdownReportingAdId = q.BreakdownReportingAdId ?? string.Empty,
                    CallToActionAsset = q.CallToActionAsset ?? string.Empty,
                    CoarseConversionValue = q.CoarseConversionValue ?? string.Empty,
                    Country = q.Country ?? string.Empty,
                    DescriptionAsset = q.DescriptionAsset ?? string.Empty,
                    FidelityType = q.FidelityType ?? string.Empty,
                    Gender = q.Gender ?? string.Empty,
                    Hsid = q.Hsid ?? string.Empty,
                    ImageAsset = q.ImageAsset ?? string.Empty,
                    ImpressionDevice = q.ImpressionDevice ?? string.Empty,
                    IsConversionIdModeled = q.IsConversionIdModeled ?? string.Empty,
                    IsRenderedAsDelayedSkipAd = q.IsRenderedAsDelayedSkipAd ?? string.Empty,
                    LandingDestination = q.LandingDestination ?? string.Empty,
                    LinkUrlAsset = q.LinkUrlAsset ?? string.Empty,
                    MdsaLandingDestination = q.MdsaLandingDestination ?? string.Empty,
                    MediaAssetUrl = q.MediaAssetUrl ?? string.Empty,
                    MediaCreator = q.MediaCreator ?? string.Empty,
                    MediaDestinationUrl = q.MediaDestinationUrl ?? string.Empty,
                    MediaFormat = q.MediaFormat ?? string.Empty,
                    MediaOriginUrl = q.MediaOriginUrl ?? string.Empty,
                    MediaTextContent = q.MediaTextContent ?? string.Empty,
                    MediaType = q.MediaType ?? string.Empty,
                    PostbackSequenceIndex = q.PostbackSequenceIndex ?? string.Empty,
                    ProductId = q.ProductId ?? string.Empty,
                    Redownload = q.Redownload ?? string.Empty,
                    Region = q.Region ?? string.Empty,
                    SkanCampaignId = q.SkanCampaignId ?? string.Empty,
                    SkanConversionId = q.SkanConversionId ?? string.Empty,
                    SkanVersion = q.SkanVersion ?? string.Empty,
                    SotAttributionModelType = q.SotAttributionModelType ?? string.Empty,
                    SotAttributionWindow = q.SotAttributionWindow ?? string.Empty,
                    SotChannel = q.SotChannel ?? string.Empty,
                    SotEventType = q.SotEventType ?? string.Empty,
                    SotSource = q.SotSource ?? string.Empty,
                    TitleAsset = q.TitleAsset ?? string.Empty,
                    UserPersonaId = q.UserPersonaId ?? string.Empty,
                    UserPersonaName = q.UserPersonaName ?? string.Empty,
                    VideoAsset = q.VideoAsset ?? string.Empty,
                    Dma = q.Dma ?? string.Empty,
                    FrequencyValue = q.FrequencyValue ?? string.Empty,
                    HourlyStatsAggregatedByAdvertiserTimeZone = q.HourlyStatsAggregatedByAdvertiserTimeZone ?? string.Empty,
                    HourlyStatsAggregatedByAudienceTimeZone = q.HourlyStatsAggregatedByAudienceTimeZone ?? string.Empty,
                    Mmm = q.Mmm ?? string.Empty,
                    PlacePageId = q.PlacePageId ?? string.Empty,
                    PublisherPlatform = q.PublisherPlatform ?? string.Empty,
                    PlatformPosition = q.PlatformPosition ?? string.Empty,
                    DevicePlatform = q.DevicePlatform ?? string.Empty,
                    StandardEventContentType = q.StandardEventContentType ?? string.Empty,
                    ConversionDestination = q.ConversionDestination ?? string.Empty,
                    SignalSourceBucket = q.SignalSourceBucket ?? string.Empty,
                    MarketingMessagesBtnName = q.MarketingMessagesBtnName ?? string.Empty
                }).ToList() ?? new List<ReportFilter>()
            };

            return Ok(new List<ReportFilterResponse> { data });
        }
    }
}
