using AdminPanel.Models.Meta.Ad;
using AdminPanel.Models.Meta.AdSet;
using AdminPanel.Models.Meta.AdvertisingAccount;
using AdminPanel.Models.Meta.Business;
using AdminPanel.Models.Meta.Campaign;
using AdminPanel.Models.Meta.Insight;
using AdminPanel.Models.Meta.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.MetaData;

namespace AdminPanel.Controllers.Meta
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private readonly ILogger<MetaController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;
        private readonly MetaData _metaData;
        private readonly DefaultValues _defaultValues;

        public MetaController(ILogger<MetaController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
            _metaData = new MetaData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet("business")]
        public ActionResult<IEnumerable<BusinessResponse>> GetBusiness()
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var business = _metaData.BusinessAdmin(accessToken.AccessToken);
            var data = new BusinessResponse
            {
                Data = business.Data?.Select(q => new Business
                {
                    Id = q.Id,
                    Name = q.Name
                }).ToList() ?? new List<Business>()
            };

            return Ok(new List<BusinessResponse> { data });
        }

        [HttpGet("advertising-accounts")]
        public ActionResult<IEnumerable<AdvertisingAccountsResponse>> GetAdvertisingAccounts(string businessId)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var advertisingAccount = _metaData.AdvertisingAccountsAdmin(accessToken.AccessToken, businessId);
            var data = new AdvertisingAccountsResponse
            {
                Data = advertisingAccount.Data?.Select(q => new AdvertisingAccount
                {
                    Id = q.Id,
                    Name = q.Name
                }).ToList() ?? new List<AdvertisingAccount>()
            };

            return Ok(new List<AdvertisingAccountsResponse> { data });
        }

        [HttpGet("ads")]
        public ActionResult<IEnumerable<AdResponse>> GetAds(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var ads = _metaData.AdsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var data = new AdResponse
            {
                Data = ads.Data?.Select(q => new Ad
                {
                    Name = q.Name,
                    Status = q.Status,
                    AdSet = new AdSet
                    {
                        Name = q.AdSet.Name,
                        BidStrategy = q.AdSet.BidStrategy,
                        DailyBudget = q.AdSet.DailyBudget,
                        UpdateTime = q.AdSet.UpdateTime,
                    },
                    Insights = new InsightResponse
                    {
                        Data = q.Insights?.Data?.Select(i => new Insight
                        {
                            Reach = i.Reach,
                            Impressions = i.Impressions,
                            Cpc = i.Cpc,
                            Cpm = i.Cpm,
                            Spend = i.Spend,
                            ConversionRateRanking = i.ConversionRateRanking,
                            EngagementRateRanking = i.EngagementRateRanking,
                            QualityRanking = i.QualityRanking,
                            DateStart = i.DateStart,
                            DateStop = i.DateStop,
                            Actions = i.Actions?.Select(action => new AdminPanel.Models.Meta.Action.Action
                            {
                                ActionType = action.ActionType,
                                Value = action.Value
                            }).ToList() ?? new List<Models.Meta.Action.Action>()
                        }).ToList() ?? new List<Insight>()
                    }
                }).ToList() ?? new List<Ad>()
            };

            return Ok(new List<AdResponse> { data });
        }

        [HttpGet("adsets")]
        public ActionResult<IEnumerable<AdSetResponse>> GetAdSets(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var adSets = _metaData.AdSetsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var data = new AdSetResponse
            {
                Data = adSets.Data?.Select(q => new AdSet
                {
                    Id = q.Id,
                    Name = q.Name,
                    Status = q.Status,
                    BidStrategy = q.BidStrategy,
                    DailyBudget = q.DailyBudget,
                    LifeTimeBudget = q.LifeTimeBudget,
                    UpdateTime = q.UpdateTime,
                    StartTime = q.StartTime,
                    EndTime = q.EndTime,
                    Insights = new InsightResponse
                    {
                        Data = q.Insights?.Data?.Select(i => new Insight
                        {
                            Reach = i.Reach,
                            Impressions = i.Impressions,
                            Cpc = i.Cpc,
                            Cpm = i.Cpm,
                            Spend = i.Spend,
                            DateStart = i.DateStart,
                            DateStop = i.DateStop,
                            Actions = i.Actions?.Select(action => new AdminPanel.Models.Meta.Action.Action
                            {
                                ActionType = action.ActionType,
                                Value = action.Value
                            }).ToList() ?? new List<Models.Meta.Action.Action>()
                        }).ToList() ?? new List<Insight>()
                    }
                }).ToList() ?? new List<AdSet>()
            };

            return Ok(new List<AdSetResponse> { data });
        }

        [HttpGet("campaigns")]
        public ActionResult<IEnumerable<CampaignResponse>> GetCampaigns(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var campaigns = _metaData.CampaignsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var data = new CampaignResponse
            {
                Data = campaigns.Data?.Select(q => new Campaign
                {
                    Id = q.Id,
                    Name = q.Name,
                    Status = q.Status,
                    AccountId = q.AccountId,
                    EndTime = q.EndTime,
                    Insights = new InsightResponse
                    {
                        Data = q.Insights?.Data?.Select(i => new Insight
                        {
                            Reach = i.Reach,
                            Impressions = i.Impressions,
                            Cpc = i.Cpc,
                            Cpm = i.Cpm,
                            Spend = i.Spend,
                            DateStart = i.DateStart,
                            DateStop = i.DateStop,
                            Actions = i.Actions?.Select(action => new AdminPanel.Models.Meta.Action.Action
                            {
                                ActionType = action.ActionType,
                                Value = action.Value
                            }).ToList() ?? new List<Models.Meta.Action.Action>()
                        }).ToList() ?? new List<Insight>()
                    }
                }).ToList() ?? new List<Campaign>()
            };

            return Ok(new List<CampaignResponse> { data });
        }

        [HttpGet("general-query")]
        public ActionResult<IEnumerable<InsightGeneralResponse>> GetInsights(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 90);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var insights = _metaData.InsightsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var data = new InsightGeneralResponse
            {
                Data = insights.Data?.Select(q => new InsightGeneral
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
                    OutboundClicks = q.OutboundClicks?.Select(action => new AdminPanel.Models.Meta.Action.Action
                    {
                        ActionType = action.ActionType,
                        Value = action.Value
                    }).ToList() ?? new List<Models.Meta.Action.Action>(),
                    OutboundClicksCtr = q.OutboundClicksCtr?.Select(action => new AdminPanel.Models.Meta.Action.Action
                    {
                        ActionType = action.ActionType,
                        Value = action.Value
                    }).ToList() ?? new List<Models.Meta.Action.Action>(),
                    Actions = q.Actions?.Select(action => new AdminPanel.Models.Meta.Action.Action
                    {
                        ActionType = action.ActionType,
                        Value = action.Value
                    }).ToList() ?? new List<Models.Meta.Action.Action>()
                }).ToList() ?? new List<InsightGeneral>()
            };

            return Ok(new List<InsightGeneralResponse> { data });
        }

        [HttpGet("report-query")]
        public ActionResult<IEnumerable<ReportFilterResponse>> GetInsightsReport(string accountId, string reportFilter, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
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

        private int UserId()
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return 0;
            }

            int userId = int.Parse(userIdClaim.Value);
            return userId;
        }
    }
}
