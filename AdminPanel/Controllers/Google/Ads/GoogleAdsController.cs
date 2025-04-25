using AdminPanel.Controllers.Google.Analytics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.Util;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.V18.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Google.Ads.GoogleAds.V18.Enums;
using Google.Ads.GoogleAds.V18.Resources;
using Google.Api.Gax;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;
using Google.Ads.Gax.Config;
using Google.Ads.GoogleAds.V17.Errors;

namespace AdminPanel.Controllers.Google.Ads
{
	[Authorize]
	[Route("api/[controller]")]
    [ApiController]
    public class GoogleAdsController : ControllerBase
    {
        private readonly ILogger<GoogleAdsController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public GoogleAdsController(ILogger<GoogleAdsController> logger, GoogleService googleService, GoogleData googleData)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService, googleData);
            _googleData = googleData;
            _defaultValues = new DefaultValues();
        }

		[HttpGet("account")]
		public IActionResult GetAccount()
		{
			var userId = UserId();
			var control = _googleTokenControl.GetTokenAds(userId);
			var app = _googleService.GetGoogleApp();

			var config = new GoogleAdsConfig()
			{
				DeveloperToken = app.DeveloperToken,
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = app.AppId,
				OAuth2ClientSecret = app.AppSecret,
				OAuth2RefreshToken = control
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V17.CustomerService);
			string[] customers = service.ListAccessibleCustomers();

			var accountDetails = new List<object>();

			foreach (var customerId in customers)
			{
				string id = customerId.Split('/')[1];

				var googleAdsService = client.GetService(Services.V18.GoogleAdsService);
				string query = $@"
					SELECT
						customer.id,
						customer.descriptive_name,
						customer.manager
					FROM
						customer
					WHERE
						customer.manager = false AND customer.id = '{id}'
				";

				var searchRequest = new SearchGoogleAdsRequest()
				{
					CustomerId = id,
					Query = query
				};
				var response = googleAdsService.Search(searchRequest);

				foreach (var row in response)
				{
					var rawId = row.Customer.Id.ToString();
					var formattedId = rawId.Length == 10
						? $"{rawId.Substring(0, 3)}-{rawId.Substring(3, 3)}-{rawId.Substring(6, 4)}"
						: rawId;

					accountDetails.Add(new
					{
						Id = row.Customer.Id,
						Name = string.IsNullOrEmpty(row.Customer.DescriptiveName) ? "Google Ads Hesabı (" + formattedId + ")" : row.Customer.DescriptiveName
					});
				}
			}

			return Ok(accountDetails);
		}

		[HttpGet("ads-account")]
		public ActionResult<IEnumerable<object>> GetOrganizationAdsAccount()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
			var adsAccount = organization.GoogleAccount;
			var result = adsAccount
				.Split(',')
				.Select(accountInfo =>
				{
					var parts = accountInfo.Split('/');
					return new
					{
						account = parts[0],
						accountId = parts[1]
					};
				})
				.ToList();

			return Ok(result);
		}

		[HttpGet("ads-summary")]
		public IActionResult GetAccountSummary(string customerId)
		{
			var userId = UserId();
			var control = _googleTokenControl.GetTokenAds(userId);
			var app = _googleService.GetGoogleApp();

			var config = new GoogleAdsConfig()
			{
				DeveloperToken = app.DeveloperToken,
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = app.AppId,
				OAuth2ClientSecret = app.AppSecret,
				OAuth2RefreshToken = control
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V18.GoogleAdsService);

			string query = @"
				SELECT
					customer.id,
					customer.descriptive_name,
					customer.currency_code,
					customer.time_zone,
					customer.manager,
					metrics.clicks,
					metrics.impressions,
					metrics.ctr,
					metrics.average_cpc,
					metrics.cost_micros,
					metrics.conversions,
					metrics.cost_per_conversion
				FROM customer";

			var request = new SearchGoogleAdsRequest()
			{
				CustomerId = customerId,
				Query = query
			};

			var response = service.Search(request);

			var summaryList = new List<object>();

			foreach (var row in response)
			{
				summaryList.Add(new
				{
					AccountId = row.Customer.Id,
					AccountName = string.IsNullOrEmpty(row.Customer.DescriptiveName) ? "Google Ads Hesabı" : row.Customer.DescriptiveName,
					Currency = row.Customer.CurrencyCode,
					TimeZone = row.Customer.TimeZone,
					IsManagerAccount = row.Customer.Manager,
					Clicks = row.Metrics.Clicks,
					Impressions = row.Metrics.Impressions,
					Ctr = row.Metrics.Ctr * 100,
					AverageCpc = row.Metrics.AverageCpc != null ? Convert.ToDouble(row.Metrics.AverageCpc) / 1_000_000.0 : 0,
					Cost = row.Metrics.CostMicros / 1_000_000.0,
					Conversions = row.Metrics.Conversions,
					CostPerConversion = row.Metrics.CostPerConversion != null ? Convert.ToDouble(row.Metrics.CostPerConversion) / 1_000_000.0 : 0
				});
			}

			return Ok(summaryList);
		}

		[HttpGet("dashboard")]
		public IActionResult GetAccountSummary()
		{
			var userId = UserId();
			var control = _googleTokenControl.GetTokenAds(userId);
			var app = _googleService.GetGoogleApp();

			var config = new GoogleAdsConfig()
			{
				DeveloperToken = app.DeveloperToken,
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = app.AppId,
				OAuth2ClientSecret = app.AppSecret,
				OAuth2RefreshToken = control
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V18.GoogleAdsService);
			var services = client.GetService(Services.V17.CustomerService);
			string[] customers = services.ListAccessibleCustomers();

			var totalClicks = 0;
			var totalImpressions = 0;
			var totalCost = 0.0;

			foreach (var customerRow in customers)
			{
				string customerId = customerRow.Split('/')[1];

				string checkMccQuery = @"
					SELECT customer.manager 
					FROM customer";

				var checkMccRequest = new SearchGoogleAdsRequest()
				{
					CustomerId = customerId,
					Query = checkMccQuery
				};

				var checkResponse = service.Search(checkMccRequest);

				bool isManager = checkResponse.FirstOrDefault()?.Customer?.Manager ?? false;

				if (isManager)
				{
					continue;
				}

				string query = @"
					SELECT
						customer.id,
						metrics.clicks,
						metrics.impressions,
						metrics.cost_micros
					FROM customer
					WHERE customer.id = " + customerId;

				var request = new SearchGoogleAdsRequest()
				{
					CustomerId = customerId,
					Query = query
				};

				var response = service.Search(request);

				foreach (var row in response)
				{
					totalClicks += (int)(row.Metrics?.Clicks ?? 0);
					totalImpressions += (int)(row.Metrics?.Impressions ?? 0);
					totalCost += (row.Metrics?.CostMicros ?? 0) / 1_000_000.0;
				}
			}

			return Ok(new
			{
				TotalClicks = totalClicks,
				TotalImpressions = totalImpressions,
				TotalCost = totalCost
			});
		}

		[HttpGet("campaigns")]
		public IActionResult GetCampaigns(string customerId)
		{
			var userId = UserId();
			var control = _googleTokenControl.GetTokenAds(userId);
			var app = _googleService.GetGoogleApp();

			var config = new GoogleAdsConfig()
			{
				DeveloperToken = app.DeveloperToken,
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = app.AppId,
				OAuth2ClientSecret = app.AppSecret,
				OAuth2RefreshToken = control
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V18.GoogleAdsService);

			string query = @"
					SELECT
					  campaign.id,
					  campaign.name,
					  campaign.status,
					  campaign.start_date,
					  campaign.end_date,
					  campaign.advertising_channel_type,
					  campaign_budget.amount_micros,
					  campaign.optimization_score,
					  campaign.advertising_channel_sub_type,
					  metrics.clicks,
					  metrics.impressions,
					  metrics.ctr,
					  metrics.average_cpc,
					  metrics.cost_micros,
					  campaign.bidding_strategy_type,
					  metrics.conversions,
					  metrics.cost_per_conversion
					FROM campaign
					WHERE campaign.status != 'REMOVED'";

			var searchRequest = new SearchGoogleAdsRequest()
			{
				CustomerId = customerId,
				Query = query
			};

			var response = service.Search(searchRequest);

			var campaignList = new List<object>();

			foreach (var row in response)
			{
				campaignList.Add(new
				{
					Id = row.Campaign.Id,
					Name = row.Campaign.Name,
					Status = row.Campaign.Status.ToString() == "Enabled" ? "Aktif" : "Pasif",
					StartDate = row.Campaign.StartDate,
					EndDate = row.Campaign.EndDate,
					ChannelType = _defaultValues.GetAdvertisingChannelTypeName(_defaultValues.ToUpperSnakeCase(row.Campaign.AdvertisingChannelType.ToString())),
					Budget = row.CampaignBudget?.AmountMicros / 1_000_000.0,
					OptimizationScore = row.Campaign.OptimizationScore * 100,
					CampaignSubType = row.Campaign.AdvertisingChannelSubType.ToString(),
					Clicks = row.Metrics.Clicks,
					Impressions = row.Metrics.Impressions,
					Ctr = row.Metrics.Ctr * 100,
					AverageCpc = row.Metrics.AverageCpc != null ? Convert.ToDouble(row.Metrics.AverageCpc) / 1_000_000.0 : 0,
					Cost = row.Metrics.CostMicros / 1_000_000.0,
					BiddingStrategyType = _defaultValues.GetBiddingStrategyTypeName(_defaultValues.ToUpperSnakeCase(row.Campaign.BiddingStrategyType.ToString())),
					Conversions = row.Metrics.Conversions,
					ConversionRate = row.Metrics.Clicks != 0 ? (row.Metrics.Conversions / row.Metrics.Clicks) * 100 : 0,
					CostPerConversion = row.Metrics.CostPerConversion != null ? Convert.ToDouble(row.Metrics.CostPerConversion) / 1_000_000.0 : 0,
				});
			}

			return Ok(campaignList);
		}

		[HttpGet("ad-groups")]
		public IActionResult GetAdGroups(string customerId)
		{
			var userId = UserId();
			var control = _googleTokenControl.GetTokenAds(userId);
			var app = _googleService.GetGoogleApp();

			var config = new GoogleAdsConfig()
			{
				DeveloperToken = app.DeveloperToken,
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = app.AppId,
				OAuth2ClientSecret = app.AppSecret,
				OAuth2RefreshToken = control
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V18.GoogleAdsService);

			string query = @"
				SELECT
				  campaign.name,
				  ad_group.id,
				  ad_group.name,
				  ad_group.status,
				  ad_group.type,
				  ad_group.cpc_bid_micros,
				  metrics.clicks,
				  metrics.impressions,
				  metrics.ctr,
				  metrics.average_cpc,
				  metrics.cost_micros,
				  metrics.conversions,
				  metrics.cost_per_conversion
				FROM ad_group
				WHERE ad_group.status != 'REMOVED' AND campaign.status != 'REMOVED'";

			var request = new SearchGoogleAdsRequest()
			{
				CustomerId = customerId,
				Query = query
			};

			var response = service.Search(request);
			var adGroupList = new List<object>();

			foreach (var row in response)
			{
				adGroupList.Add(new
				{
					CampaignName = row.Campaign.Name,
					AdGroupId = row.AdGroup.Id,
					AdGroupName = row.AdGroup.Name,
					Status = row.AdGroup.Status.ToString() == "Enabled" ? "Aktif" : "Pasif",
					Type = _defaultValues.GetAdGroupTypeName(row.AdGroup.Type.ToString()),
					Clicks = row.Metrics.Clicks,
					Impressions = row.Metrics.Impressions,
					Ctr = row.Metrics.Ctr * 100,
					AverageCpc = row.Metrics.AverageCpc != null ? Convert.ToDouble(row.Metrics.AverageCpc) / 1_000_000.0 : 0,
					Cost = row.Metrics.CostMicros / 1_000_000.0,
					Conversions = row.Metrics.Conversions,
					ConversionRate = row.Metrics.Clicks != 0 ? (row.Metrics.Conversions / row.Metrics.Clicks) * 100 : 0,
					CostPerConversion = row.Metrics.CostPerConversion != null ? Convert.ToDouble(row.Metrics.CostPerConversion) / 1_000_000.0 : 0,
					TargetEbm = row.Metrics.Conversions != 0 ? (row.Metrics.CostMicros / 1_000_000.0) / row.Metrics.Conversions : 0
				});
			}

			return Ok(adGroupList);
		}

		[HttpGet("ads")]
		public IActionResult GetAds(string customerId)
		{
			var userId = UserId();
			var control = _googleTokenControl.GetTokenAds(userId);
			var app = _googleService.GetGoogleApp();

			var config = new GoogleAdsConfig()
			{
				DeveloperToken = app.DeveloperToken,
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = app.AppId,
				OAuth2ClientSecret = app.AppSecret,
				OAuth2RefreshToken = control
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V18.GoogleAdsService);

			string query = @"
				SELECT
				  campaign.name,
				  ad_group.name,
				  ad_group_ad.ad.id,
				  ad_group_ad.ad.responsive_search_ad.headlines,
				  ad_group_ad.ad.responsive_search_ad.descriptions,
				  ad_group_ad.ad.final_urls,
				  ad_group_ad.status,
				  ad_group_ad.ad.type,
				  ad_group_ad.ad_strength,
				  metrics.clicks,
				  metrics.impressions,
				  metrics.ctr,
				  metrics.average_cpc,
				  metrics.cost_micros,
				  metrics.conversions,
				  metrics.cost_per_conversion
				FROM ad_group_ad
				WHERE ad_group_ad.status != 'REMOVED'
				  AND ad_group.status != 'REMOVED'
				  AND campaign.status != 'REMOVED'";

			var request = new SearchGoogleAdsRequest()
			{
				CustomerId = customerId,
				Query = query
			};

			var response = service.Search(request);
			var adList = new List<object>();

			foreach (var row in response)
			{
				var responsiveAd = row.AdGroupAd.Ad.ResponsiveSearchAd;
				string headline = responsiveAd?.Headlines.Count > 0 ? responsiveAd.Headlines[0].Text : "";
				string description = responsiveAd?.Descriptions.Count > 0 ? responsiveAd.Descriptions[0].Text : "";

				adList.Add(new
				{
					Name = headline + " / " + description + " " + (row.AdGroupAd.Ad.FinalUrls.Count > 0 ? row.AdGroupAd.Ad.FinalUrls[0] : null),
					AdId = row.AdGroupAd.Ad.Id,
					CampaignName = row.Campaign.Name,
					AdGroupName = row.AdGroup.Name,
					Status = row.AdGroupAd.Status.ToString() == "Enabled" ? "Aktif" : "Pasif",
					AdType = _defaultValues.GetAdTypeName(row.AdGroupAd.Ad.Type.ToString()),
					AdStrength = _defaultValues.GetAdStrengthName(row.AdGroupAd.AdStrength.ToString()),
					Headline = headline,
					Description = description,
					FinalUrl = row.AdGroupAd.Ad.FinalUrls.Count > 0 ? row.AdGroupAd.Ad.FinalUrls[0] : null,
					Clicks = row.Metrics.Clicks,
					Impressions = row.Metrics.Impressions,
					Ctr = row.Metrics.Ctr * 100,
					AverageCpc = row.Metrics.AverageCpc != null ? Convert.ToDouble(row.Metrics.AverageCpc) / 1_000_000.0 : 0,
					Cost = row.Metrics.CostMicros / 1_000_000.0,
					Conversions = row.Metrics.Conversions,
					ConversionRate = row.Metrics.Clicks != 0 ? (row.Metrics.Conversions / row.Metrics.Clicks) * 100 : 0,
					CostPerConversion = row.Metrics.CostPerConversion != null ? Convert.ToDouble(row.Metrics.CostPerConversion) / 1_000_000.0 : 0,
				});
			}

			return Ok(adList);
		}

		[HttpGet("ads-keywords")]
		public IActionResult GetAdKeywords(string customerId)
		{
			var userId = UserId();
			var control = _googleTokenControl.GetTokenAds(userId);
			var app = _googleService.GetGoogleApp();

			var config = new GoogleAdsConfig()
			{
				DeveloperToken = app.DeveloperToken,
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = app.AppId,
				OAuth2ClientSecret = app.AppSecret,
				OAuth2RefreshToken = control
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V18.GoogleAdsService);

			string query = @"
				SELECT
				  campaign.name,
				  ad_group.name,
				  ad_group.id,
				  ad_group_criterion.criterion_id,
				  ad_group_criterion.keyword.text,
				  ad_group_criterion.keyword.match_type,
				  ad_group_criterion.status,
				  campaign.serving_status,
				  metrics.clicks,
				  metrics.impressions,
				  metrics.ctr,
				  metrics.average_cpc,
				  metrics.cost_micros,
				  metrics.conversions,
				  metrics.cost_per_conversion
				FROM keyword_view
				WHERE campaign.status != 'REMOVED'
				  AND ad_group.status != 'REMOVED'
				  AND ad_group_criterion.status != 'REMOVED'
				  ORDER BY metrics.clicks DESC";

			var request = new SearchGoogleAdsRequest()
			{
				CustomerId = customerId,
				Query = query
			};

			var response = service.Search(request);
			var keywords = new List<object>();

			foreach (var row in response)
			{
				keywords.Add(new
				{
					CampaignName = row.Campaign.Name,
					AdGroupName = row.AdGroup.Name,
					AdGroupId = row.AdGroup.Id,
					CriterionId = row.AdGroupCriterion.CriterionId,
					KeywordText = row.AdGroupCriterion.Keyword.Text,
					MatchType = _defaultValues.GetKeywordMatchTypeName(row.AdGroupCriterion.Keyword.MatchType.ToString()),
					Status = row.AdGroupCriterion.Status.ToString() == "Enabled" ? "Aktif" : "Pasif",
					ChangeStatus = _defaultValues.GetSystemServingStatusName(row.Campaign.ServingStatus.ToString()),
					Clicks = row.Metrics.Clicks,
					Impressions = row.Metrics.Impressions,
					Ctr = row.Metrics.Ctr * 100,
					AverageCpc = row.Metrics.AverageCpc != null ? Convert.ToDouble(row.Metrics.AverageCpc) / 1_000_000.0 : 0,
					Cost = row.Metrics.CostMicros / 1_000_000.0,
					Conversions = row.Metrics.Conversions,
					ConversionRate = row.Metrics.Clicks != 0 ? (row.Metrics.Conversions / row.Metrics.Clicks) * 100 : 0,
					CostPerConversion = row.Metrics.CostPerConversion != null ? Convert.ToDouble(row.Metrics.CostPerConversion) / 1_000_000.0 : 0,
				});
			}

			return Ok(keywords);
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
