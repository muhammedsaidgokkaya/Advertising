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
using Google.Ads.GoogleAds.V18.Common;
using static AdminPanel.Models.Google.Ads.AddCampaign;
using static AdminPanel.Models.Google.Ads.AddAdSet;
using static AdminPanel.Models.Google.Ads.AddAd;
using static Google.Ads.GoogleAds.V18.Enums.AdGroupAdStatusEnum.Types;
using Google.Protobuf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Google.Api;

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
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
			var adsAccount = organization.GoogleAccount ?? string.Empty;
			var accounts = adsAccount.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(account =>
				{
					var parts = account.Split('/');
					return new
					{
						account = parts[0],
						accountId = long.Parse(parts[1])
					};
				})
				.ToList();

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

			var accountDetails = new List<GoogleAccountDto>();

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

					accountDetails.Add(new GoogleAccountDto
					{
						Id = row.Customer.Id,
						Name = string.IsNullOrEmpty(row.Customer.DescriptiveName) ? "Google Ads Hesabı (" + formattedId + ")" : row.Customer.DescriptiveName
					});
				}
			}

			var accountIds = accounts.Select(a => a.accountId).ToHashSet();
			var availableAccounts = accountDetails
				.Where(a => !accountIds.Contains(a.Id))
				.ToList();

			var selectedAccounts = accountDetails
				.Where(a => accountIds.Contains(a.Id))
				.ToList();

			var responses = new
			{
				Available = availableAccounts,
				Selected = selectedAccounts
			};

			return Ok(responses);
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
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
			var totalClicks = 0;
			var totalImpressions = 0;
			var totalCost = 0.0;
			var adsAccount = organization.GoogleAccount;

            if (adsAccount != null)
            {
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
				var accountIds = result.Select(r => r.accountId).ToList();
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

				if (accountIds != null)
				{
					foreach (var customerId in accountIds)
					{
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
				}

				return Ok(new
				{
					TotalClicks = totalClicks,
					TotalImpressions = totalImpressions,
					TotalCost = totalCost
				});
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

		[HttpGet("ad-groups-campaign")]
		public IActionResult GetCampaignAdGroups(string customerId, string campaignId)
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

            string query = $@"
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
				WHERE ad_group.status != 'REMOVED'
				  AND campaign.status != 'REMOVED'
				  AND campaign.id = {campaignId}";

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

        [HttpGet("asset-groups")]
        public IActionResult GetAssetGroups(string customerId)
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
			        asset_group.id,
			        asset_group.name,
			        asset_group.status,
			        metrics.clicks,
		            metrics.impressions,
		            metrics.ctr,
		            metrics.average_cpc,
		            metrics.cost_micros,
		            metrics.conversions,
		            metrics.cost_per_conversion
		        FROM asset_group
		        WHERE campaign.status != 'REMOVED'
		          AND asset_group.status != 'REMOVED'";

            var request = new SearchGoogleAdsRequest()
            {
                CustomerId = customerId,
                Query = query
            };

            var response = service.Search(request);
            var assetGroups = new List<object>();

            foreach (var row in response)
            {
                assetGroups.Add(new
                {
                    CampaignName = row.Campaign.Name,
                    AssetGroupId = row.AssetGroup.Id,
                    AssetGroupName = row.AssetGroup.Name,
                    Status = row.AssetGroup.Status.ToString() == "Enabled" ? "Aktif" : "Pasif",
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

            return Ok(assetGroups);
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

        [HttpGet("geo")]
        public async Task<IActionResult> SuggestGeoTargetConstants(string query)
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
            var service = client.GetService(Services.V18.GeoTargetConstantService);

            var request = new SuggestGeoTargetConstantsRequest
            {
                Locale = "tr",
                LocationNames = new SuggestGeoTargetConstantsRequest.Types.LocationNames
                {
                    Names = { query }
                }
            };

            var response = await service.SuggestGeoTargetConstantsAsync(request);

            var result = response.GeoTargetConstantSuggestions.Select(x => new LocationSuggestion
            {
                Id = x.GeoTargetConstant.Id,
                Name = $"{x.GeoTargetConstant.CanonicalName}, {GetTurkishTargetType(x.GeoTargetConstant.TargetType)}",
                CountryCode = x.GeoTargetConstant.CountryCode,
                TargetType = x.GeoTargetConstant.TargetType
            }).ToList();

            return Ok(result);
        }

        [HttpPost("save-search-campaign")]
        public async Task<IActionResult> SaveSearchCampaign([FromBody] SearchCampaignRequest request)
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
            var budgetService = client.GetService(Services.V18.CampaignBudgetService);
            var campaignService = client.GetService(Services.V18.CampaignService);

            if (!long.TryParse(request.Budget, out long budgetValue))
            {
                throw new Exception("Budget değeri geçersiz.");
            }

            long amountMicros = budgetValue * 1_000_000;

            var budget = new CampaignBudget
            {
                Name = $"{Guid.NewGuid()}",
                AmountMicros = amountMicros,
                DeliveryMethod = BudgetDeliveryMethodEnum.Types.BudgetDeliveryMethod.Standard,
                ExplicitlyShared = false
            };

            var budgetOp = new CampaignBudgetOperation { Create = budget };
            var budgetResponse = await budgetService.MutateCampaignBudgetsAsync(
                request.SelectedAccountId.ToString(), new[] { budgetOp });

            string budgetResource = budgetResponse.Results[0].ResourceName;

            AdvertisingChannelTypeEnum.Types.AdvertisingChannelType deliveryMethod;

            switch (request.SelectedType?.ToLower())
            {
                case "search":
                    deliveryMethod = AdvertisingChannelTypeEnum.Types.AdvertisingChannelType.Search;
                    break;
                case "performance":
                    deliveryMethod = AdvertisingChannelTypeEnum.Types.AdvertisingChannelType.PerformanceMax;
                    break;
                case "dısplay":
                    deliveryMethod = AdvertisingChannelTypeEnum.Types.AdvertisingChannelType.Display;
                    break;
                default:
                    deliveryMethod = AdvertisingChannelTypeEnum.Types.AdvertisingChannelType.Search;
                    break;
            }

            var biddingStrategyService = client.GetService(Services.V18.BiddingStrategyService);
            var operations = new List<BiddingStrategyOperation>();

            if (request.BiddingType == "maxConversions" || request.BiddingType == "")
            {
                var strategy = new BiddingStrategy
                {
                    Name = $"MaximizeConversions-{Guid.NewGuid()}",
                    Type = BiddingStrategyTypeEnum.Types.BiddingStrategyType.MaximizeConversions
                };

                if (request.TargetCpa != "")
                {
                    strategy.MaximizeConversions = new MaximizeConversions
                    {
                        TargetCpaMicros = (long)(decimal.Parse(request.TargetCpa) * 1_000_000)
                    };
                }

                operations.Add(new BiddingStrategyOperation { Create = strategy });
            }
            else if (request.BiddingType == "targetROAS")
			{
				var strategy = new BiddingStrategy
				{
                    Name = $"TargetROAS-{Guid.NewGuid()}",
                    TargetRoas = new TargetRoas
                    {
                        TargetRoas_ = string.IsNullOrEmpty(request.TargetRoas)
							? 0.0
							: double.Parse(request.TargetRoas.Replace("%", "")) / 100.0
                    },
                    Type = BiddingStrategyTypeEnum.Types.BiddingStrategyType.TargetRoas
                };

				operations.Add(new BiddingStrategyOperation { Create = strategy });
			}
			else if (request.BiddingType == "clicks")
			{
				var strategy = new BiddingStrategy
				{
                    Name = $"MaximizeClicks-{Guid.NewGuid()}",
                    TargetSpend = new TargetSpend
                    {
                        CpcBidCeilingMicros = string.IsNullOrEmpty(request.MaxCpcLimit)
							? 0L
							: (long)(decimal.Parse(request.MaxCpcLimit) * 1_000_000)
                    },
                    Type = BiddingStrategyTypeEnum.Types.BiddingStrategyType.TargetSpend
                };

				operations.Add(new BiddingStrategyOperation { Create = strategy });
			}
            else if (request.BiddingType == "impressionShare")
            {
                var positionEnum = request.ImpressionPosition switch
                {
                    "anywhere" => TargetImpressionShareLocationEnum.Types.TargetImpressionShareLocation.AnywhereOnPage,
                    "top" => TargetImpressionShareLocationEnum.Types.TargetImpressionShareLocation.TopOfPage,
                    "veryTop" => TargetImpressionShareLocationEnum.Types.TargetImpressionShareLocation.AbsoluteTopOfPage,
                    _ => TargetImpressionShareLocationEnum.Types.TargetImpressionShareLocation.AnywhereOnPage
                };

                var targetImpressionShare = new TargetImpressionShare
                {
                    Location = positionEnum,
                    LocationFractionMicros = (long)(decimal.Parse(request.ImpressionShareTarget) * 10_000)
                };

                if (!string.IsNullOrEmpty(request.MaxCpcImpressionLimit))
                {
                    targetImpressionShare.CpcBidCeilingMicros = (long)(decimal.Parse(request.MaxCpcImpressionLimit) * 1_000_000);
                }

                var strategy = new BiddingStrategy
                {
                    Name = $"TargetImpressionShare-{Guid.NewGuid()}",
                    TargetImpressionShare = targetImpressionShare,
                    Type = BiddingStrategyTypeEnum.Types.BiddingStrategyType.TargetImpressionShare
                };

                operations.Add(new BiddingStrategyOperation { Create = strategy });
            }

            var responses = await biddingStrategyService.MutateBiddingStrategiesAsync(
                request.SelectedAccountId.ToString(), operations);

            var biddingResourceName = responses.Results.First().ResourceName;

            var campaign = new Campaign
            {
                Name = request.CampaignName,
                AdvertisingChannelType = deliveryMethod,
                Status = CampaignStatusEnum.Types.CampaignStatus.Paused,
                CampaignBudget = budgetResource,
                StartDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd"),
                EndDate = DateTime.Now.AddYears(3).ToString("yyyyMMdd")
            };

            if (deliveryMethod == AdvertisingChannelTypeEnum.Types.AdvertisingChannelType.PerformanceMax)
            {
                if (request.BiddingType == "maxConversions")
                {
                    campaign.BiddingStrategyType = BiddingStrategyTypeEnum.Types.BiddingStrategyType.MaximizeConversions;

                    if (request.TargetCpa != "")
                    {
                        campaign.MaximizeConversions = new MaximizeConversions
                        {
                            TargetCpaMicros = (long)(decimal.Parse(request.TargetCpa) * 1_000_000)
                        };
                    }
                }
                else if (request.BiddingType == "targetROAS")
                {
                    campaign.BiddingStrategyType = BiddingStrategyTypeEnum.Types.BiddingStrategyType.MaximizeConversionValue;

                    if (request.TargetRoas != "")
                    {
                        campaign.MaximizeConversionValue = new MaximizeConversionValue
                        {
                            TargetRoas = string.IsNullOrEmpty(request.TargetRoas)
                            ? 0.0
                            : double.Parse(request.TargetRoas.Replace("%", "")) / 100.0
                        };
                    }
                }
            }
            else if (deliveryMethod == AdvertisingChannelTypeEnum.Types.AdvertisingChannelType.Display)
            {
                if (request.BiddingType == "maxConversions")
                {
                    campaign.BiddingStrategyType = BiddingStrategyTypeEnum.Types.BiddingStrategyType.TargetCpa;

                    if (request.TargetCpa != "")
                    {
                        campaign.TargetCpa = new TargetCpa
                        {
                            TargetCpaMicros = (long)(decimal.Parse(request.TargetCpa) * 1_000_000)
                        };
                    }
                }
                else if (request.BiddingType == "targetROAS")
                {
                    campaign.BiddingStrategyType = BiddingStrategyTypeEnum.Types.BiddingStrategyType.MaximizeConversionValue;

                    if (request.TargetRoas != "")
                    {
                        campaign.MaximizeConversionValue = new MaximizeConversionValue
                        {
                            TargetRoas = double.Parse(request.TargetRoas.Replace("%", "")) / 100.0
                        };
                    }
                }
            }
            else
            {
                campaign.BiddingStrategy = biddingResourceName;
            }

            var campaignOp = new CampaignOperation { Create = campaign };
            var campaignResponse = await campaignService.MutateCampaignsAsync(
                request.SelectedAccountId.ToString(), new[] { campaignOp });

            string campaignResource = campaignResponse.Results[0].ResourceName;

            var campaignCriterionService = client.GetService(Services.V18.CampaignCriterionService);

            if (request.Locations == "all")
            {
                
            }

            if (request.Locations == "turkey")
            {
                var locationCriterion = new CampaignCriterion
                {
                    Campaign = campaignResource,
                    Location = new LocationInfo
                    {
                        GeoTargetConstant = ResourceNames.GeoTargetConstant(2792)
                    }
                };

                var locationOp = new CampaignCriterionOperation { Create = locationCriterion };
                var locationResponse = await campaignCriterionService.MutateCampaignCriteriaAsync(
                    request.SelectedAccountId.ToString(), new[] { locationOp });
            }

            if (request.Locations == "custom")
            {
                foreach (var item in request.CustomLocations)
                {
                    var locationCriterion = new CampaignCriterion
                    {
                        Campaign = campaignResource,
                        Location = new LocationInfo
                        {
                            GeoTargetConstant = ResourceNames.GeoTargetConstant(item)
                        }
                    };

                    var locationOp = new CampaignCriterionOperation { Create = locationCriterion };
                    var locationResponse = await campaignCriterionService.MutateCampaignCriteriaAsync(
                        request.SelectedAccountId.ToString(), new[] { locationOp });
                }
            }

            if (request.SelectedLanguages != null && request.SelectedLanguages.Count > 0)
            {
                foreach (var item in request.SelectedLanguages)
                {
                    var languageCriterion = new CampaignCriterion
                    {
                        Campaign = campaignResource,
                        Language = new LanguageInfo
                        {
                            LanguageConstant = ResourceNames.LanguageConstant(item)
                        }
                    };

                    var languageOp = new CampaignCriterionOperation { Create = languageCriterion };
                    var languageResponse = await campaignCriterionService.MutateCampaignCriteriaAsync(
                        request.SelectedAccountId.ToString(), new[] { languageOp });
                }
            }

            //var googleAdsServices = client.GetService(Services.V18.GoogleAdsService);

            //var querys = @"
            //    SELECT 
            //        geo_target_constant.resource_name, 
            //        geo_target_constant.name, 
            //        geo_target_constant.country_code, 
            //        geo_target_constant.target_type, 
            //        geo_target_constant.status 
            //    FROM geo_target_constant 
            //    WHERE geo_target_constant.target_type = 'Country' 
            //        AND geo_target_constant.status = 'ENABLED'
            //    ORDER BY geo_target_constant.name";

            //var requestss = new SearchGoogleAdsRequest
            //{
            //    CustomerId = request.SelectedAccountId.ToString(),
            //    Query = querys
            //};

            //var result = new List<object>();

            //foreach (var row in googleAdsServices.Search(requestss))
            //{
            //    var geo = row.GeoTargetConstant;
            //    result.Add(new
            //    {
            //        Id = geo.ResourceName.Split('/')[1],
            //        Name = geo.Name,
            //        CountryCode = geo.CountryCode
            //    });
            //}

            return Ok(1);
        }

        [HttpPost("save-search-adset")]
        public async Task<IActionResult> SaveSearchAdSet([FromBody] CampaignSaveRequest request)
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
            var adGroupService = client.GetService(Services.V18.AdGroupService);

            AdGroupTypeEnum.Types.AdGroupType adGroupType;
			adGroupType = AdGroupTypeEnum.Types.AdGroupType.SearchStandard;
			if (request.SelectedCampaignType == "Arama Ağı")
			{
				adGroupType = AdGroupTypeEnum.Types.AdGroupType.SearchStandard;
            }
			if (request.SelectedCampaignType == "Görüntülü Reklam Ağı")
			{
                adGroupType = AdGroupTypeEnum.Types.AdGroupType.DisplayStandard;
            }

            var adGroup = new AdGroup
            {
                Name = request.AdGroupName,
                Campaign = "customers/" + request.SelectedAccountId + "/campaigns/" + request.SelectedCampaignId.ToString(),
                Status = AdGroupStatusEnum.Types.AdGroupStatus.Enabled,
                Type = adGroupType
            };

            var adGroupOperation = new AdGroupOperation { Create = adGroup };
            var adGroupResponse = await adGroupService.MutateAdGroupsAsync(request.SelectedAccountId, new[] { adGroupOperation });

            string adGroupResourceName = adGroupResponse.Results[0].ResourceName;

            var adGroupCriterionService = client.GetService(Services.V18.AdGroupCriterionService);

            var operations = new List<AdGroupCriterionOperation>();

            if (request.Chips != null && request.Chips.Count > 0)
            {
                foreach (var keyword in request.Chips)
                {
                    var keywordInfo = new KeywordInfo
                    {
                        Text = keyword,
                        MatchType = KeywordMatchTypeEnum.Types.KeywordMatchType.Broad
                    };

                    var adGroupCriterion = new AdGroupCriterion
                    {
                        AdGroup = adGroupResourceName,
                        Status = AdGroupCriterionStatusEnum.Types.AdGroupCriterionStatus.Enabled,
                        Keyword = keywordInfo
                    };

                    operations.Add(new AdGroupCriterionOperation { Create = adGroupCriterion });
                }

                var response = await adGroupCriterionService.MutateAdGroupCriteriaAsync(
                    request.SelectedAccountId, operations.ToArray());

                var addedKeywords = response.Results.Select(r => r.ResourceName).ToList();
            }
           
            return Ok(1);
        }

        [HttpPost("save-search-ad")]
        public async Task<IActionResult> SaveSearchAd([FromBody] SaveRequestModel request)
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

            ResponsiveSearchAdInfo responsiveSearchAd = new ResponsiveSearchAdInfo()
            {
                Headlines = { request.Headlines.Select(h => new AdTextAsset { Text = h }) },
                Descriptions = { request.Descriptions?.Select(d => new AdTextAsset { Text = d }) },
            };

            if (request.Url1 != null && request.Url1 != "")
            {
                responsiveSearchAd.Path1 = request.Url1;
            }

            if (request.Url2 != null && request.Url2 != "")
            {
                responsiveSearchAd.Path2 = request.Url2;
            }

            AdGroupAdOperation operation = new AdGroupAdOperation()
            {
                Create = new AdGroupAd()
                {
                    AdGroup = "customers/" + request.SelectedAccountId + "/adGroups/" + request.SelectedAdGroupId.ToString(),
                    Status = AdGroupAdStatus.Enabled,
                    Ad = new Ad()
                    {
                        Name = request.AdName,
                        ResponsiveSearchAd = responsiveSearchAd,
                        FinalUrls = { request.FinalUrl }
                    }
                }
            };

            AdGroupAdServiceClient serviceClient = client.GetService(Services.V18.AdGroupAdService);

            MutateAdGroupAdsResponse response = serviceClient.MutateAdGroupAds(
                request.SelectedAccountId,
                new[] { operation }.ToList()
            );

            string resourceName = response.Results[0].ResourceName;

            return Ok(1);
        }

        [HttpPost("save-display-ad")]
        public async Task<IActionResult> SaveDisplayAd([FromForm] SaveDisplayRequestModel request)
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

            var adGroupAdService = client.GetService(Services.V18.AdGroupAdService);
            
			var assetService = client.GetService(Services.V18.AssetService);
            var assetResourceImageNames = new List<string>();

            if (request.Images != null && request.Images.Count != 0)
            {
                foreach (var imageFile in request.Images)
                {
                    using var ms = new MemoryStream();
                    await imageFile.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();

                    using var originalImage = Image.Load(imageBytes);
                    int targetWidth = 1910;
                    int targetHeight = (int)(targetWidth / 1.91);

                    originalImage.Mutate(x =>
                    {
                        x.Resize(new ResizeOptions
                        {
                            Size = new Size(targetWidth, targetHeight),
                            Mode = ResizeMode.Crop,
                            Position = AnchorPositionMode.Center
                        });
                    });

                    using var outStream = new MemoryStream();
                    originalImage.SaveAsJpeg(outStream);
                    var resizedImageBytes = outStream.ToArray();

                    var asset = new Asset
                    {
                        Name = "Görsel" + Guid.NewGuid().ToString(),
                        ImageAsset = new ImageAsset
                        {
                            Data = ByteString.CopyFrom(resizedImageBytes)
                        },
                        Type = AssetTypeEnum.Types.AssetType.Image
                    };

                    var operations = new AssetOperation
                    {
                        Create = asset
                    };

                    var responses = await assetService.MutateAssetsAsync(request.SelectedAccountId, new[] { operations });
                    assetResourceImageNames.Add(responses.Results[0].ResourceName);
                }
            }

            var assetResourceSquareMarketingImagesNames = new List<string>();

            if (request.Images != null && request.Images.Count != 0)
            {
                foreach (var logoFile in request.Images)
                {
                    using var ms = new System.IO.MemoryStream();
                    await logoFile.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();

                    using var originalImage = Image.Load(imageBytes);

                    if (originalImage.Width < 512 || originalImage.Height < 512)
                    {
                        originalImage.Mutate(x =>
                        {
                            x.Resize(new ResizeOptions
                            {
                                Size = new Size(512, 512),
                                Mode = ResizeMode.Pad,
                                Position = AnchorPositionMode.Center,
                                PadColor = Color.White
                            });
                        });
                    }
                    else
                    {
                        originalImage.Mutate(x =>
                        {
                            x.Resize(new ResizeOptions
                            {
                                Size = new Size(512, 512),
                                Mode = ResizeMode.Crop,
                                Position = AnchorPositionMode.Center
                            });
                        });
                    }

                    using var outStream = new MemoryStream();
                    originalImage.SaveAsPng(outStream);
                    var resizedImageBytes = outStream.ToArray();

                    var asset = new Asset
                    {
                        Name = "Görsel" + Guid.NewGuid().ToString(),
                        ImageAsset = new ImageAsset
                        {
                            Data = ByteString.CopyFrom(resizedImageBytes)
                        },
                        Type = AssetTypeEnum.Types.AssetType.Image
                    };

                    var operations = new AssetOperation
                    {
                        Create = asset
                    };

                    var responses = await assetService.MutateAssetsAsync(request.SelectedAccountId, new[] { operations });
                    assetResourceSquareMarketingImagesNames.Add(responses.Results[0].ResourceName);
                }
            }

            var assetResourceLogoNames = new List<string>();

            if (request.Logos != null && request.Logos.Count != 0)
			{
                foreach (var logoFile in request.Logos)
                {
                    using var ms = new System.IO.MemoryStream();
                    await logoFile.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();

                    using var originalImage = Image.Load(imageBytes);
                    int targetWidth = 1200;
                    int targetHeight = 300;

                    originalImage.Mutate(x =>
                    {
                        x.Resize(new ResizeOptions
                        {
                            Size = new Size(targetWidth, targetHeight),
                            Mode = ResizeMode.Crop,
                            Position = AnchorPositionMode.Center
                        });
                    });

                    using var outStream = new MemoryStream();
                    originalImage.SaveAsJpeg(outStream);
                    var resizedImageBytes = outStream.ToArray();

                    var asset = new Asset
                    {
                        Name = "Logo" + Guid.NewGuid().ToString(),
                        ImageAsset = new ImageAsset
                        {
                            Data = ByteString.CopyFrom(resizedImageBytes)
                        },
                        Type = AssetTypeEnum.Types.AssetType.Image
                    };

                    var operations = new AssetOperation
                    {
                        Create = asset
                    };

                    var responses = await assetService.MutateAssetsAsync(request.SelectedAccountId, new[] { operations });
                    assetResourceLogoNames.Add(responses.Results[0].ResourceName);
                }
            }

            var responsiveDisplayAdInfo = new ResponsiveDisplayAdInfo
            {
                Headlines = { request.Headlines.Select(h => new AdTextAsset { Text = h }) },
                Descriptions = { request.Descriptions?.Select(d => new AdTextAsset { Text = d }) },
                LongHeadline = new AdTextAsset { Text = request.LongTittle },
                BusinessName = request.AccountName,
            };

            if (assetResourceImageNames != null && assetResourceImageNames.Count > 0)
            {
                responsiveDisplayAdInfo.MarketingImages.AddRange(
                    assetResourceImageNames.Select(name => new AdImageAsset { Asset = name })
                );
            }

            if (assetResourceSquareMarketingImagesNames != null && assetResourceSquareMarketingImagesNames.Count > 0)
            {
                responsiveDisplayAdInfo.SquareMarketingImages.AddRange(
                    assetResourceSquareMarketingImagesNames.Select(name => new AdImageAsset { Asset = name })
                );
            }

            if (assetResourceLogoNames != null && assetResourceLogoNames.Count > 0)
            {
                responsiveDisplayAdInfo.LogoImages.AddRange(
                    assetResourceLogoNames.Select(name => new AdImageAsset { Asset = name })
                );
            }

            var ad = new Ad
            {
				Name = request.AdName,
                ResponsiveDisplayAd = responsiveDisplayAdInfo,
                FinalUrls = { request.FinalUrl }
            };

            var adGroupAd = new AdGroupAd
            {
                AdGroup = ResourceNames.AdGroup(long.Parse(request.SelectedAccountId), request.SelectedAdGroupId),
                Ad = ad,
                Status = AdGroupAdStatusEnum.Types.AdGroupAdStatus.Paused
            };

            var operation = new AdGroupAdOperation
            {
                Create = adGroupAd
            };

            var response = adGroupAdService.MutateAdGroupAds(request.SelectedAccountId, new[] { operation });
			return Ok(1);
        }

        [HttpPost("save-max-ad")]
        public async Task<IActionResult> SaveMaxAdUnified([FromForm] SaveMaxRequestModel request)
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
            var mutateOperations = new List<MutateOperation>();
            long customerId = long.Parse(request.SelectedAccountId);
            long campaignId = request.SelectedCampaignId;

            int tempIdCounter = -1;

            var headlineAssetIds = request.Headlines.Select(h =>
            {
                var op = new MutateOperation
                {
                    AssetOperation = new AssetOperation
                    {
                        Create = new Asset
                        {
                            ResourceName = ResourceNames.Asset(customerId, tempIdCounter),
                            TextAsset = new TextAsset { Text = h },
                            Type = AssetTypeEnum.Types.AssetType.Text
                        }
                    }
                };
                mutateOperations.Add(op);
                return tempIdCounter--;
            }).ToList();

            var descriptionAssetIds = request.Descriptions.Select(d =>
            {
                var op = new MutateOperation
                {
                    AssetOperation = new AssetOperation
                    {
                        Create = new Asset
                        {
                            ResourceName = ResourceNames.Asset(customerId, tempIdCounter),
                            TextAsset = new TextAsset { Text = d },
                            Type = AssetTypeEnum.Types.AssetType.Text
                        }
                    }
                };
                mutateOperations.Add(op);
                return tempIdCounter--;
            }).ToList();

            var longHeadlineAssetIds = request.LongHeadlines.Select(d =>
            {
                var op = new MutateOperation
                {
                    AssetOperation = new AssetOperation
                    {
                        Create = new Asset
                        {
                            ResourceName = ResourceNames.Asset(customerId, tempIdCounter),
                            TextAsset = new TextAsset { Text = d },
                            Type = AssetTypeEnum.Types.AssetType.Text
                        }
                    }
                };
                mutateOperations.Add(op);
                return tempIdCounter--;
            }).ToList();

            var businessNameAssetId = tempIdCounter;
            mutateOperations.Add(new MutateOperation
            {
                AssetOperation = new AssetOperation
                {
                    Create = new Asset
                    {
                        ResourceName = ResourceNames.Asset(customerId, businessNameAssetId),
                        TextAsset = new TextAsset { Text = request.AccountName },
                        Type = AssetTypeEnum.Types.AssetType.Text
                    }
                }
            });
            tempIdCounter--;

            var marketingImageTempIds = new List<int>();
            if (request.Images != null)
            {
                foreach (var image in request.Images)
                {
                    var imageBytes = await GetResizedImageBytes(image, 1200, 627);
                    var marketingImageId = tempIdCounter;

                    mutateOperations.Add(new MutateOperation
                    {
                        AssetOperation = new AssetOperation
                        {
                            Create = new Asset
                            {
                                Name = "Image" + Guid.NewGuid(),
                                ResourceName = ResourceNames.Asset(customerId, marketingImageId),
                                ImageAsset = new ImageAsset { Data = ByteString.CopyFrom(imageBytes) },
                                Type = AssetTypeEnum.Types.AssetType.Image
                            }
                        }
                    });

                    marketingImageTempIds.Add(marketingImageId);
                    tempIdCounter--;
                }
            }

            var squareImageTempIds = new List<int>();
            if (request.Images != null)
            {
                foreach (var image in request.Images)
                {
                    var squareImageBytes = await GetResizedImageBytes(image, 600, 600);
                    var squareImageId = tempIdCounter;

                    mutateOperations.Add(new MutateOperation
                    {
                        AssetOperation = new AssetOperation
                        {
                            Create = new Asset
                            {
                                Name = "Image" + Guid.NewGuid(),
                                ResourceName = ResourceNames.Asset(customerId, squareImageId),
                                ImageAsset = new ImageAsset { Data = ByteString.CopyFrom(squareImageBytes) },
                                Type = AssetTypeEnum.Types.AssetType.Image
                            }
                        }
                    });

                    squareImageTempIds.Add(squareImageId);
                    tempIdCounter--;
                }
            }

            var logoImageTempIds = new List<int>();
            if (request.Logos != null)
            {
                foreach (var logo in request.Logos)
                {
                    var logoImageBytes = await GetResizedImageBytes(logo, 512, 512);
                    var logoImageId = tempIdCounter;

                    mutateOperations.Add(new MutateOperation
                    {
                        AssetOperation = new AssetOperation
                        {
                            Create = new Asset
                            {
                                Name = "Logo" + Guid.NewGuid(),
                                ResourceName = ResourceNames.Asset(customerId, logoImageId),
                                ImageAsset = new ImageAsset { Data = ByteString.CopyFrom(logoImageBytes) },
                                Type = AssetTypeEnum.Types.AssetType.Image
                            }
                        }
                    });

                    logoImageTempIds.Add(logoImageId);
                    tempIdCounter--;
                }
            }

            var assetGroupTempId = -1000;
            var assetGroupResourceName = ResourceNames.AssetGroup(customerId, assetGroupTempId);
            mutateOperations.Add(new MutateOperation
            {
                AssetGroupOperation = new AssetGroupOperation
                {
                    Create = new AssetGroup
                    {
                        ResourceName = assetGroupResourceName,
                        Name = request.AdName,
                        Campaign = ResourceNames.Campaign(customerId, campaignId),
                        FinalUrls = { request.FinalUrl },
                        Status = AssetGroupStatusEnum.Types.AssetGroupStatus.Paused
                    }
                }
            });

            if (request.VideoUrls.FirstOrDefault() != null)
            {
                var youtubeVideoIds = request.VideoUrls?.Select(ExtractYoutubeVideoId).Where(id => !string.IsNullOrEmpty(id)).ToList();
                var youtubeVideoTempIds = new List<int>();

                if (youtubeVideoIds != null)
                {
                    foreach (var videoId in youtubeVideoIds)
                    {
                        var videoTempId = tempIdCounter;

                        mutateOperations.Add(new MutateOperation
                        {
                            AssetOperation = new AssetOperation
                            {
                                Create = new Asset
                                {
                                    ResourceName = ResourceNames.Asset(customerId, videoTempId),
                                    YoutubeVideoAsset = new YoutubeVideoAsset
                                    {
                                        YoutubeVideoId = videoId
                                    },
                                    Type = AssetTypeEnum.Types.AssetType.YoutubeVideo
                                }
                            }
                        });

                        youtubeVideoTempIds.Add(videoTempId);
                        tempIdCounter--;
                    }
                }
                mutateOperations.AddRange(youtubeVideoTempIds.Select(id =>
                CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, id), AssetFieldTypeEnum.Types.AssetFieldType.YoutubeVideo)));
            }

            mutateOperations.AddRange(headlineAssetIds.Select(id => CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, id), AssetFieldTypeEnum.Types.AssetFieldType.Headline)));
            mutateOperations.AddRange(descriptionAssetIds.Select(id => CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, id), AssetFieldTypeEnum.Types.AssetFieldType.Description)));
            mutateOperations.AddRange(longHeadlineAssetIds.Select(id => CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, id), AssetFieldTypeEnum.Types.AssetFieldType.LongHeadline)));
            mutateOperations.Add(CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, businessNameAssetId), AssetFieldTypeEnum.Types.AssetFieldType.BusinessName));
            mutateOperations.AddRange(marketingImageTempIds.Select(id =>
                CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, id), AssetFieldTypeEnum.Types.AssetFieldType.MarketingImage)));

            mutateOperations.AddRange(squareImageTempIds.Select(id =>
                CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, id), AssetFieldTypeEnum.Types.AssetFieldType.SquareMarketingImage)));

            mutateOperations.AddRange(logoImageTempIds.Select(id =>
                CreateAssetGroupAssetOp(assetGroupResourceName, ResourceNames.Asset(customerId, id), AssetFieldTypeEnum.Types.AssetFieldType.Logo)));

            var mutateService = client.GetService(Services.V18.GoogleAdsService);
            var response = await mutateService.MutateAsync(new MutateGoogleAdsRequest
            {
                CustomerId = customerId.ToString(),
                MutateOperations = { mutateOperations }
            });

            return Ok(1);
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

        private MutateOperation CreateAssetGroupAssetOp(string assetGroupResourceName, string assetResourceName, AssetFieldTypeEnum.Types.AssetFieldType fieldType)
        {
            return new MutateOperation
            {
                AssetGroupAssetOperation = new AssetGroupAssetOperation
                {
                    Create = new AssetGroupAsset
                    {
                        AssetGroup = assetGroupResourceName,
                        Asset = assetResourceName,
                        FieldType = fieldType
                    }
                }
            };
        }

        private async Task<byte[]> GetResizedImageBytes(IFormFile file, int width, int height)
        {
            using var inputStream = new MemoryStream();
            await file.CopyToAsync(inputStream);
            inputStream.Position = 0;

            using var image = Image.Load(inputStream.ToArray());
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop
            }));

            using var outputStream = new MemoryStream();
            image.SaveAsJpeg(outputStream);
            return outputStream.ToArray();
        }

        private string ExtractYoutubeVideoId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }

        private string GetTurkishTargetType(string targetType)
        {
            return targetType switch
            {
                "City" => "şehir",
                "Country" => "ülke",
                "Region" => "bölge",
                "State" => "eyalet",
                "Province" => "il",
                "Prefecture" => "il",
                "PostalCode" => "posta kodu",
                "District" => "ilçe",
                "Municipality" => "belediye",
                _ => targetType.ToLower()
            };
        }

        public class GoogleAccountDto
		{
			public long Id { get; set; }
			public string Name { get; set; }
		}

        public class LocationSuggestion
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string CountryCode { get; set; }
            public string TargetType { get; set; }
        }
    }
}
