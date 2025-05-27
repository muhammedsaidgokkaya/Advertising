using AdminPanel.Models.Google.Ads;
using AdminPanel.Models.Meta.Ad;
using AdminPanel.Models.Meta.AdSet;
using AdminPanel.Models.Meta.AdvertisingAccount;
using AdminPanel.Models.Meta.Audience;
using AdminPanel.Models.Meta.Business;
using AdminPanel.Models.Meta.Campaign;
using AdminPanel.Models.Meta.Charts;
using AdminPanel.Models.Meta.Insight;
using AdminPanel.Models.Meta.Report;
using AdminPanel.Models.Organization.User;
using Core.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI.API.Embedding;
using Service.Implementations.Meta;
using Service.Implementations.User;
using System.Net.Http;
using System.Text;
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
        private readonly Utilities.Utilities.MetaData.Meta _meta;
        private readonly DefaultValues _defaultValues;
        private readonly PythonRun _pythonRun;
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public MetaController(ILogger<MetaController> logger, Utilities.Utilities.MetaData.Meta meta, MetaService metaService, MetaData metaData, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _userService = new UserService();
            _httpClient = new HttpClient();
            _metaService = metaService;
            _meta = meta;
            _metaData = metaData;
            _defaultValues = new DefaultValues();
            _pythonRun = new PythonRun(); 
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("business")]
        public async Task<ActionResult<IEnumerable<BusinessResponse>>> GetBusiness()
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var business = await _meta.GetFacebookBusinesses(accessToken.AccessToken);
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

        [HttpGet("advertising-account")]
        public async Task<ActionResult<IEnumerable<AdvertisingAccountsResponse>>> GetAdvertisingAccount(string businessId)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var advertisingAccount = await _meta.GetFacebookAdAccounts(accessToken.AccessToken, businessId);
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

        [HttpGet("meta-account")]
        public ActionResult<IEnumerable<object>> GetOrganizationMetaAccount()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            var metaAccount = organization.MetaAccount;
            var accounts = metaAccount.Split(',')
            .Select(account =>
            {
                var parts = account.Split('/');
                return new
                {
                    AccountId = parts[0].Trim(),
                    Account = parts.Length > 1 ? parts[1].Trim() : string.Empty
                };
            })
            .ToList();

            return Ok(accounts);
        }

        [HttpGet("advertising-accounts")]
		public async Task<ActionResult<IEnumerable<AdvertisingAccountsResponse>>> GetAdvertisingAccounts()
        {
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
			var metaAccount = organization.MetaAccount ?? string.Empty;
			var accounts = metaAccount.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(account =>
				{
					var parts = account.Split('/');
					return new
					{
						AccountId = parts[0].Trim(),
						Account = parts.Length > 1 ? parts[1].Trim() : string.Empty
					};
				})
				.ToList();

			var accessToken = _metaService.GetLongAccessToken(userId);
			var business = await _meta.GetFacebookBusinesses(accessToken.AccessToken);
			var businessIdList = business.Data?.Select(b => b.Id).ToList() ?? new List<string>();

			var tasks = businessIdList.Select(businessId => _meta.GetFacebookAdAccounts(accessToken.AccessToken, businessId));
			var advertisingAccountsList = await System.Threading.Tasks.Task.WhenAll(tasks);

			var allAdvertisingAccounts = advertisingAccountsList
				.SelectMany(advertisingAccount => advertisingAccount.Data?.Select(q => new AdvertisingAccount
				{
					Id = q.Id,
					Name = q.Name
				}).ToList() ?? new List<AdvertisingAccount>())
				.ToList();

			var accountIds = accounts.Select(a => a.AccountId).ToHashSet();
			var availableAccounts = allAdvertisingAccounts
				.Where(a => !accountIds.Contains(a.Id))
				.ToList();

			var selectedAccounts = allAdvertisingAccounts
				.Where(a => accountIds.Contains(a.Id))
				.ToList();

			var response = new
			{
				Available = availableAccounts,
				Selected = selectedAccounts
			};

			return Ok(response);
		}

        [HttpGet("ads")]
        public ActionResult<IEnumerable<Ad>> GetAds(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var ads = _metaData.AdsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var data = ads.Data?.Select(q => new Ad
            {
                Id = q.Id,
                Name = q.Name,
                Status = q.Status == "ACTIVE" ? "Aktif" : "Pasif",
                Img = q.Creative.ThumbnailUrl,
                AdSet = new AdSet
                {
                    Name = q.AdSet.Name,
                    BidStrategy = _defaultValues.GetFormattedBidStrategy(q.AdSet.BidStrategy),
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
                        ConversionRateRanking = _defaultValues.FormatRanking(i.ConversionRateRanking),
                        EngagementRateRanking = _defaultValues.FormatRanking(i.EngagementRateRanking),
                        QualityRanking = _defaultValues.FormatRanking(i.QualityRanking),
                        DateStart = i.DateStart,
                        DateStop = i.DateStop,
                        ResultString = _defaultValues.ProcessResults(
                            q.Campaign.Objective,
                            i.Actions.Select(a => new Utilities.Helper.DefaultValues.Action
                            {
                                ActionType = a.ActionType,
                                Value = a.Value
                            })
                        ),
                        ResultDouble = _defaultValues.ProcessResultsInt(
                            q.Campaign.Objective,
                            i.Actions.Select(a => new Utilities.Helper.DefaultValues.Action
                            {
                                ActionType = a.ActionType,
                                Value = a.Value
                            })
                        )
                    }).ToList() ?? new List<Insight>()
                }
            }).ToList() ?? new List<Ad>();

            return Ok(data);
        }

        [HttpGet("adsets")]
        public ActionResult<IEnumerable<AdSet>> GetAdSets(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var adSets = _metaData.AdSetsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var data = adSets.Data?.Select(q => new AdSet
            {
                Id = q.Id,
                Name = q.Name,
                Status = q.Status == "ACTIVE" ? "Aktif" : "Pasif",
                BidStrategy = _defaultValues.GetFormattedBidStrategy(q.BidStrategy),
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
                        ResultString = _defaultValues.ProcessResults(
                            q.Campaign.Objective,
                            i.Actions.Select(a => new Utilities.Helper.DefaultValues.Action
                            {
                                ActionType = a.ActionType,
                                Value = a.Value
                            })
                        ),
                        ResultDouble = _defaultValues.ProcessResultsInt(
                            q.Campaign.Objective,
                            i.Actions.Select(a => new Utilities.Helper.DefaultValues.Action
                            {
                                ActionType = a.ActionType,
                                Value = a.Value
                            })
                        )
                    }).ToList() ?? new List<Insight>()
                }
            }).ToList() ?? new List<AdSet>();

            return Ok(data);
        }

        [HttpGet("campaigns")]
        public ActionResult<IEnumerable<Campaign>> GetCampaigns(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var campaigns = _metaData.CampaignsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var data = campaigns.Data?.Select(q => new Campaign
            {
                Id = q.Id,
                Name = q.Name,
                Status = q.Status == "ACTIVE" ? "Aktif" : "Pasif",
                AccountId = q.AccountId,
                EndTime = q.EndTime,
                BuyingType = q.BuyingType,
                Objective = q.Objective,
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
                        ResultString = _defaultValues.ProcessResults(
                            q.Objective,
                            i.Actions.Select(a => new Utilities.Helper.DefaultValues.Action
                            {
                                ActionType = a.ActionType,
                                Value = a.Value
                            })
                        ),
                        ResultDouble = _defaultValues.ProcessResultsInt(
                            q.Objective,
                            i.Actions.Select(a => new Utilities.Helper.DefaultValues.Action
                            {
                                ActionType = a.ActionType,
                                Value = a.Value
                            })
                        ),
                    }).ToList() ?? new List<Insight>()
                }
            }).ToList() ?? new List<Campaign>();

            return Ok(data);
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

        [HttpGet("charts")]
        public ActionResult<ApiResponse> GetCharts(string accountId)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var defaultValues = _defaultValues.DefaultMounth();
            var charts = _metaData.Charts(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            return Ok(charts);
        }

        [HttpGet("dashboards")]
		public async Task<ActionResult<DashboardMeta>> GetDashboards()
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);

			var metaAccount = organization.MetaAccount;

			if (metaAccount != null)
			{
				var accounts = metaAccount.Split(',')
				.Select(account =>
				{
					var parts = account.Split('/');
					return new
					{
						AccountId = parts[0].Trim(),
						Account = parts.Length > 1 ? parts[1].Trim() : string.Empty
					};
				})
				.ToList();

				var accountIds = accounts.Select(r => r.AccountId).ToList();

				var charts = await _meta.GetCombinedMetricsAsync(accessToken.AccessToken, accountIds);
				var data = new DashboardMeta
				{
					Spend = charts.Spend,
					Impressions = charts.Impressions,
					Clicks = charts.Clicks
				};

				return Ok(data);
			}

			return Ok(new
			{
				Spend = 0,
				Impressions = 0,
				Clicks = 0
			});
        }

		[HttpGet("top-ads")]
		public async Task<ActionResult<List<TopAds>>> GetTopAds()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);

			var metaAccount = organization.MetaAccount;

			if (metaAccount != null)
            {
				var accounts = metaAccount.Split(',')
				.Select(account =>
				{
					var parts = account.Split('/');
					return new
					{
						AccountId = parts[0].Trim(),
						Account = parts.Length > 1 ? parts[1].Trim() : string.Empty
					};
				})
				.ToList();

				var accountIds = accounts.Select(r => r.AccountId).ToList();
				var accessToken = _metaService.GetLongAccessToken(userId);

				var topAds = await _meta.GetAllAdsAsync(accessToken.AccessToken, accountIds);

				var data = topAds
					.Select((a, index) => new TopAds
					{
						Id = index + 1,
						Name = a.Name,
						Url = a.ImageUrl
					})
					.ToList();

				return Ok(data);
			}

            return Ok(new List<TopAds>
            {
                new TopAds
                {
                    Id = 1,
                    Name = "Henüz reklam bulunamadı",
                    Url = ""
                }
            });
        }

		[HttpGet("audiences")]
        public async Task<ActionResult<ApiResponse>> GetAudiences(string accountId)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var audiencesTask = _metaData.Audiences(accessToken.AccessToken, accountId);
            var savedAudiencesTask = _metaData.SavedAudiences(accessToken.AccessToken, accountId);

            await System.Threading.Tasks.Task.WhenAll(audiencesTask, savedAudiencesTask);

            var audiences = audiencesTask.Result;
            var savedAudiences = savedAudiencesTask.Result;
            var allAudiences = audiences.Select(a => new Audience
            {
                Id = a.Id,
                Name = a.Name,
                ApproximateCountUpperBound = a.ApproximateCountUpperBound,
                ApproximateCountLowerBound = a.ApproximateCountLowerBound,
                TargetAudienceSize = _defaultValues.GetTargetAudienceSize(a.ApproximateCountUpperBound, a.ApproximateCountLowerBound),
                AudienceType = a.AudienceType,
                AudienceTypeText = _defaultValues.GetAudienceTypeText(a.AudienceType),
                TimeCreated = a.TimeCreated,
                TimeUpdated = a.TimeUpdated,
                Gender = "—",
                AgeRange = "—",
                Countries = "—"
            }).ToList();

            var allSavedAudiences = savedAudiences.Select(s => new Audience
            {
                Id = s.Id,
                Name = s.Name,
                ApproximateCountUpperBound = s.ApproximateCountUpperBound,
                ApproximateCountLowerBound = s.ApproximateCountLowerBound,
                TargetAudienceSize = _defaultValues.GetTargetAudienceSize(s.ApproximateCountUpperBound, s.ApproximateCountLowerBound),
                AudienceType = s.AudienceType,
                AudienceTypeText = _defaultValues.GetAudienceTypeText(s.AudienceType),
                TimeCreated = DateTime.Parse(s.TimeCreated),
                TimeUpdated = DateTime.Parse(s.TimeUpdated),
                Gender = s.Targeting?.Genders != null ? string.Join(", ", s.Targeting.Genders.Select(gender => _defaultValues.GetGenderString(gender))) : "—",
                AgeRange = s.Targeting?.AgeRange != null ? _defaultValues.GetAgeRangeString(s.Targeting.AgeRange[0], s.Targeting.AgeRange[1]) : "—",
                Countries = s.Targeting?.GeoLocations?.Cities != null ? string.Join(", ", s.Targeting.GeoLocations.Cities.Select(c => _defaultValues.GetCountryNameFormat(c.Country, c.Name))) : "—",
            }).ToList();

            allAudiences.AddRange(allSavedAudiences);

            return Ok(allAudiences);
        }

        [HttpGet("selected-audiences")]
        public async Task<ActionResult<ApiResponse>> GetSelectedAudiences(string accountId)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var audiencesTask = _metaData.Audiences(accessToken.AccessToken, accountId);
            var savedAudiencesTask = _metaData.SavedAudiences(accessToken.AccessToken, accountId);

            await System.Threading.Tasks.Task.WhenAll(audiencesTask, savedAudiencesTask);

            var audiences = audiencesTask.Result;
            var savedAudiences = savedAudiencesTask.Result;
            var allAudiences = audiences.Select(a => new Audience
            {
                Id = a.Id,
                Name = a.Name,
                AudienceType = a.AudienceType
            }).ToList();

            var allSavedAudiences = savedAudiences.Select(s => new Audience
            {
                Id = s.Id,
                Name = s.Name,
                AudienceType = s.AudienceType
            }).ToList();

            allAudiences.AddRange(allSavedAudiences);

            return Ok(allAudiences);
        }

        [HttpGet("facebook-pages")]
        public async Task<IActionResult> GetFacebookPages()
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var url = $"https://graph.facebook.com/v18.0/me/accounts?access_token={accessToken.AccessToken}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<FacebookPageResponse>(url);
                return Ok(response?.Data ?? new List<FacebookPage>());
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Facebook API hatası: {ex.Message}");
            }
        }

        [HttpGet("instagram-account")]
        public async Task<IActionResult> GetInstagramAccount(string facebookId)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var url = $"https://graph.facebook.com/v18.0/{facebookId}?fields=connected_instagram_account&access_token={accessToken.AccessToken}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<InstagramAccount>(url);
                return Ok(response?.Connected_Instagram_Account);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Instagram API hatası: {ex.Message}");
            }
        }

        [HttpGet("get-pixels")]
        public async Task<IActionResult> GetPixels(string adAccountId)
        {
            var accessToken = _metaService.GetLongAccessToken(UserId());

            var businessUrl = $"https://graph.facebook.com/v19.0/{adAccountId}?fields=business&access_token={accessToken.AccessToken}";
            var businessResponse = await _httpClient.GetAsync(businessUrl);
            var businessContent = await businessResponse.Content.ReadAsStringAsync();

            dynamic businessData = JsonConvert.DeserializeObject(businessContent);
            string businessId = businessData?.business?.id;

            if (string.IsNullOrEmpty(businessId))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Business ID alınamadı. Access token veya ad account yetkisi yetersiz olabilir."
                });
            }

            var pixelsUrl = $"https://graph.facebook.com/v19.0/{businessId}/owned_pixels?access_token={accessToken.AccessToken}";
            var pixelsResponse = await _httpClient.GetAsync(pixelsUrl);
            var pixelsContent = await pixelsResponse.Content.ReadAsStringAsync();

            dynamic pixelsData = JsonConvert.DeserializeObject(pixelsContent);

            var pixelIds = new List<string>();
            foreach (var pixel in pixelsData?.data)
            {
                pixelIds.Add((string)pixel.id);
            }

            return Ok(pixelIds);
        }

        [HttpPost("create-campaign")]
        public async Task<IActionResult> CreateCampaign([FromBody] Models.Meta.Campaign.AddCampaign request)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var url = $"https://graph.facebook.com/v19.0/" + request.SelectedAccountId + "/campaigns";

            var basePayload = new Dictionary<string, object>
            {
                { "name", request.CampaignName },
                { "objective", request.SelectedType },
                { "status", "PAUSED" },
                { "special_ad_categories", new[] { "NONE" } },
                { "buying_type", request.SelectedPriceType },
                { "access_token", accessToken.AccessToken }
            };

            if (request.SelectedPriceType == "AUCTION")
            {
                basePayload["campaign_budget_optimization"] = request.AdvantageBudget;

                if (request.AdvantageBudget)
                {
                    if (request.Daily?.ToLower() == "day")
                    {
                        basePayload["daily_budget"] = int.TryParse(request.Budget, out var daily) ? (daily * 100) : 10000;
                        basePayload["bid_strategy"] = request.BidStrategy;
                    }
                    else if (request.Daily?.ToLower() == "total")
                    {
                        basePayload["lifetime_budget"] = int.TryParse(request.Budget, out var lifetime) ? (lifetime * 100) : 50000;
                        basePayload["bid_strategy"] = request.BidStrategy;
                    }
                }
            }
           
            var json = System.Text.Json.JsonSerializer.Serialize(basePayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var campaignId = await response.Content.ReadAsStringAsync();

            return Ok(campaignId);
        }

        [HttpPost("create-adset")]
        public async Task<IActionResult> CreateAdSet([FromBody] Models.Meta.AdSet.AddAdSet.AdSetDto request)
        {
            var userId = UserId();
            var accessToken = _metaService.GetLongAccessToken(userId);

            string predictionId = null;
            if (request.SelectedCampaignType == "RESERVED")
            {
                var predictionUrl = $"https://graph.facebook.com/v19.0/" + request.SelectedAccountId + "/reachfrequencypredictions";

                var targetingT = new Dictionary<string, object>
                {
                    { "geo_locations", new { countries = new[] { "TR" } } },
                    { "genders", new[] { 1, 2 } },
                    { "age_min", 18 },
                    { "age_max", 65 }
                };

                var predictionPayload = new Dictionary<string, object>
                {
                    { "campaign_id", request.SelectedCampaignId },
                    { "objective", request.SelectedCampaignObjectiveType },
                    { "start_time", ToUnixTimestamp(request.StartDate) },
                    { "frequency_cap", 2 },
                    { "targeting_spec", targetingT },
                    { "budget", int.TryParse(request.Budget, out var budget) ? (budget * 100) : 50000 },
                    { "access_token", accessToken.AccessToken }
                };

                var predictionJson = JsonConvert.SerializeObject(predictionPayload);
                var predictionContent = new StringContent(predictionJson, Encoding.UTF8, "application/json");

                var predictionResponse = await _httpClient.PostAsync(predictionUrl, predictionContent);
                var predictionResultText = await predictionResponse.Content.ReadAsStringAsync();

                dynamic predictionResult = JsonConvert.DeserializeObject(predictionResultText);
                if (predictionResult?.id == null)
                {
                    return Ok(new
                    {
                        Success = false,
                        title = "Tahmin Başarısız",
                        message = "Tahmin oluşturulamadı."
                    });
                }

                predictionId = predictionResult.id;
            }

            var url = $"https://graph.facebook.com/v19.0/" + request.SelectedAccountId + "/adsets";

            var basePayload = new Dictionary<string, object>
            {
                { "name", request.AdSetName },
                { "start_time", request.StartDate.ToString("yyyy-MM-ddTHH:mm:sszzz") },
                { "campaign_id", request.SelectedCampaignId },
                { "page_id", request.SelectedFacebookPageId },
                { "status", "PAUSED" },
                { "access_token", accessToken.AccessToken }
            };

            if (!string.IsNullOrEmpty(predictionId))
            {
                basePayload["rf_prediction_id"] = predictionId;
            }

            if (request.SelectedInstagramAccountId != "0")
            {
                basePayload.Add("instagram_actor_id", request.SelectedInstagramAccountId);
            }

            basePayload.Add("billing_event", request.BillingEvent);

            Dictionary<string, object> targeting;

            if (request.IsAdvantage)
            {
                basePayload.Add("automatic_placements", true);
                targeting = new Dictionary<string, object>();
            }
            else
            {
                targeting = new Dictionary<string, object>
                {
                    { "publisher_platforms", request.PublisherPlatforms },
                };

                if (request.FacebookPositions != null && request.FacebookPositions.Any())
                {
                    var facebookPositions = new List<string>(request.FacebookPositions);

                    var requiresFeed = new[] { "marketplace", "search", "profile_feed", "notification" };
                    if (facebookPositions.Any(x => requiresFeed.Contains(x)) && !facebookPositions.Contains("feed"))
                    {
                        facebookPositions.Add("feed");
                    }

                    targeting.Add("facebook_positions", facebookPositions);
                }

                if (request.InstagramPositions != null && request.InstagramPositions.Any())
                {
                    var instagramPositions = new List<string>(request.InstagramPositions);

                    if (instagramPositions.Contains("explore") ||
                        !instagramPositions.Contains("profile_feed") ||
                        !instagramPositions.Contains("stream"))
                    {
                        instagramPositions.Add("stream");
                    }

                    targeting.Add("instagram_positions", instagramPositions);
                }

                if (request.AudienceNetworkPositions != null && request.AudienceNetworkPositions.Any())
                {
                    targeting.Add("audience_network_positions", request.AudienceNetworkPositions);
                }

                if (request.MessengerPositions != null && request.MessengerPositions.Any())
                {
                    var messengerPositions = new List<string>(request.MessengerPositions);

                    if (messengerPositions.Contains("messenger_home"))
                    {
                        if (!request.PublisherPlatforms.Contains("facebook"))
                        {
                            request.PublisherPlatforms.Add("facebook");
                        }

                        if (request.FacebookPositions == null)
                            request.FacebookPositions = new List<string>();

                        if (!request.FacebookPositions.Contains("feed"))
                        {
                            request.FacebookPositions.Add("feed");
                        }

                        targeting["facebook_positions"] = request.FacebookPositions;
                    }

                    if (messengerPositions.Contains("story"))
                    {
                        if (!request.PublisherPlatforms.Contains("facebook") && !request.PublisherPlatforms.Contains("instagram"))
                        {
                            request.PublisherPlatforms.Add("facebook");
                        }
                    }

                    if (messengerPositions.Contains("sponsored_messages") && messengerPositions.Count > 1)
                    {
                        messengerPositions = new List<string> { "sponsored_messages" };
                    }

                    targeting.Add("messenger_positions", messengerPositions);
                }
            }

            if (request.SelectedCampaignType == "AUCTION")
            {
                switch (request.SelectedAudienceType?.ToLower())
                {
                    case "custom":
                        targeting.Add("custom_audiences", new[]
                        {
                        new Dictionary<string, object> { { "id", request.SelectedAudienceId } }
                    });
                        break;

                    case "lookalike":
                        targeting.Add("lookalike_audiences", new[]
                        {
                        new Dictionary<string, object> { { "id", request.SelectedAudienceId } }
                    });
                        break;

                    case "saved":
                        targeting.Add("saved_audience", request.SelectedAudienceId);
                        break;

                    default:
                        throw new Exception("Geçersiz audience türü: " + request.SelectedAudienceType);
                }
            }

            basePayload.Add("targeting", targeting);

            if (request.SelectedCampaignObjectiveType != "OUTCOME_AWARENESS" && request.SelectedCampaignObjectiveType != "OUTCOME_ENGAGEMENT")
            {
                var conversionConfig = new Dictionary<string, object>
                {
                    { "event_type", request.ConversionEvent },
                    { "pixel_id", request.SelectedPixelId }
                };

                basePayload.Add("conversion_configuration", new
                {
                    conversions = new[] { conversionConfig }
                });

                basePayload["promoted_object"] = new Dictionary<string, object>
                {
                    { "pixel_id", request.SelectedPixelId },
                    { "custom_event_type", request.ConversionEvent }
                };
            }

            if (request.SelectedCampaignObjectiveType == "OUTCOME_ENGAGEMENT")
            {
                basePayload["optimization_goal"] = "REACH";
            }

            if (request.EndDate != null)
            {
                basePayload.Add("end_time", request.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            }

            if (request.SelectedCampaignType == "AUCTION")
            {
                if (request.BidStrategy != "LOWEST_COST_WITHOUT_CAP")
                {
                    basePayload.Add("bid_amount", int.TryParse(request.BidFinished, out var bidFinished) ? (bidFinished * 100) : 10000);
                }

                if (request.Daily?.ToLower() == "day")
                {
                    basePayload["daily_budget"] = int.TryParse(request.Budget, out var daily) ? (daily * 100) : 10000;
                    basePayload["bid_strategy"] = request.BidStrategy;
                }
                else if (request.Daily?.ToLower() == "total")
                {
                    basePayload["lifetime_budget"] = int.TryParse(request.Budget, out var lifetime) ? (lifetime * 100) : 50000;
                    basePayload["bid_strategy"] = request.BidStrategy;
                }
            }

            var json = System.Text.Json.JsonSerializer.Serialize(basePayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            var responseText = Encoding.UTF8.GetString(responseBytes);

            dynamic error = JsonConvert.DeserializeObject(responseText);
            if (error?.error != null)
            {
                string userTitle = error.error.error_user_title;
                string userMsg = error.error.error_user_msg;

                return Ok(new
                {
                    Success = false,
                    title = userTitle,
                    message = userMsg
                });
            }

            var adSetId = await response.Content.ReadAsStringAsync();

            return Ok(new
            {
                Success = true,
                Id = adSetId
            });
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

        private long ToUnixTimestamp(DateTime date) =>
            ((DateTimeOffset)date).ToUnixTimeSeconds();

        public class FacebookPageResponse
        {
            public List<FacebookPage> Data { get; set; }
        }

        public class FacebookPage
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string AccessToken { get; set; }
        }

        public class InstagramAccount
        {
            public ConnectedInstagramAccount Connected_Instagram_Account { get; set; }
        }

        public class ConnectedInstagramAccount
        {
            public string Id { get; set; }
            public string Username { get; set; }
        }
    }
}
