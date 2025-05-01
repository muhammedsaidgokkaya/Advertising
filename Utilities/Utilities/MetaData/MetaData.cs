using Core.Domain.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Utilities.Helper;
using Utilities.Utilities.MetaData.MetaModel;
using static Utilities.Utilities.GoogleData.GoogleData;
using static Utilities.Utilities.MetaData.MetaData;

namespace Utilities.Utilities.MetaData
{
    public class MetaData
    {
        private readonly PythonRun _pythonRun;
        private readonly IConfiguration _configuration;

        public MetaData(IConfiguration configuration)
        {
            _pythonRun = new PythonRun();
            _configuration = configuration;
        }

        private string GetPythonScriptPath(string relativePath)
        {
            string basePath = _configuration["PythonScriptBasePath"];
            return Path.Combine(basePath, relativePath);
        }

        #region Meta
        public AccessTokenResponse LongAccessTokenAdmin(string app_id, string app_secret, string short_lived_token)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/AccessToken/longAccessTokenAdmin.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, app_id, app_secret, short_lived_token);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public CampaignResponse CampaignsAdmin(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/AdvertisingManager/campaigns.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<CampaignResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public AdSetResponse AdSetsAdmin(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/AdvertisingManager/adsets.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<AdSetResponse>(jsonOutput.ToString());
                if (tokenResponse?.Data != null)
                {
                    foreach (var adSet in tokenResponse.Data)
                    {
                        adSet.DailyBudget /= 100;
                    }
                }
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public AdResponse AdsAdmin(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/AdvertisingManager/ads.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<AdResponse>(jsonOutput.ToString());
                if (tokenResponse?.Data != null)
                {
                    foreach (var adSet in tokenResponse.Data)
                    {
                        adSet.AdSet.DailyBudget /= 100;
                    }
                }
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public InsightGeneralResponse InsightsAdmin(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/Report/insights.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<InsightGeneralResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }
        
        public ReportFilterResponse InsightsFilterAdmin(string access_token, string ad_account_id, string start_date, string end_date, string filterParams)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/Report/insgihtsFilter.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date, filterParams);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<ReportFilterResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public ApiResponse Charts(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/Charts/charts.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonOutput?.ToString() ?? string.Empty);
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public async Task<List<Audience>> Audiences(string access_token, string ad_account_id)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/AdvertisingManager/audience.py");
            var jsonOutput = await Task.Run(() => _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id));

            try
            {
                return JsonConvert.DeserializeObject<List<Audience>>(jsonOutput?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.", ex);
            }
        }

        public async Task<List<SavedAudience>> SavedAudiences(string access_token, string ad_account_id)
        {
            string pythonScriptPath = GetPythonScriptPath("Meta/AdvertisingManager/savedAudience.py");
            var jsonOutput = await Task.Run(() => _pythonRun.RunPythonScript(pythonScriptPath, access_token, ad_account_id));

            try
            {
                return JsonConvert.DeserializeObject<List<SavedAudience>>(jsonOutput?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.", ex);
            }
        }
		#endregion

		#region Class
		public class AccessTokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }

        public class BusinessResponse
        {
            [JsonProperty("data")]
            public List<Business> Data { get; set; }
        }

        public class Business
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class AdvertisingAccountsResponse
        {
            [JsonProperty("data")]
            public List<AdvertisingAccounts> Data { get; set; }
        }

        public class AdvertisingAccounts
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class CampaignResponse
        {
            [JsonProperty("data")]
            public List<Campaign> Data { get; set; }
        }

        public class Campaign
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("objective")]
            public string Objective { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("account_id")]
            public string AccountId { get; set; }

            [JsonProperty("insights")]
            public InsightResponse Insights { get; set; }

            [JsonProperty("end_time")]
            public DateTime? EndTime { get; set; }
        }

        public class AdSetResponse
        {
            [JsonProperty("data")]
            public List<AdSet> Data { get; set; }
        }

        public class AdSet
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("bid_strategy")]
            public string BidStrategy { get; set; }

            [JsonProperty("daily_budget")]
            public int DailyBudget { get; set; }

            [JsonProperty("lifetime_budget")]
            public int LifeTimeBudget { get; set; }

            [JsonProperty("campaign")]
            public Campaign Campaign { get; set; }

            [JsonProperty("updated_time")]
            public DateTime? UpdateTime { get; set; }

            [JsonProperty("start_time")]
            public DateTime? StartTime { get; set; }

            [JsonProperty("end_time")]
            public DateTime? EndTime { get; set; }

            [JsonProperty("insights")]
            public InsightResponse Insights { get; set; }
        }

        public class AdResponse
        {
            [JsonProperty("data")]
            public List<Ad> Data { get; set; }
        }

        public class Ad
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("configured_status")]
            public string Status { get; set; }

            [JsonProperty("adset")]
            public AdSet AdSet { get; set; }

            [JsonProperty("campaign")]
            public Campaign Campaign { get; set; }

            [JsonProperty("creative")]
            public Creative Creative { get; set; }

            [JsonProperty("insights")]
            public InsightResponse Insights { get; set; }
        }

        public class Creative
        {
            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl { get; set; }
        }

        public class InsightResponse
        {
            [JsonProperty("data")]
            public List<Insight> Data { get; set; }
        }
        
        public class InsightGeneralResponse
        {
            [JsonProperty("data")]
            public List<InsightGeneral> Data { get; set; }
        }

        public class Insight
        {
            [JsonProperty("reach")]
            public int Reach { get; set; }

            [JsonProperty("impressions")]
            public int Impressions { get; set; }

            [JsonProperty("cpc")]
            public double Cpc { get; set; }

            [JsonProperty("cpm")]
            public double Cpm { get; set; }

            [JsonProperty("spend")]
            public double Spend { get; set; }

            [JsonProperty("quality_ranking")]
            public string QualityRanking { get; set; }

            [JsonProperty("engagement_rate_ranking")]
            public string EngagementRateRanking { get; set; }

            [JsonProperty("conversion_rate_ranking")]
            public string ConversionRateRanking { get; set; }

            [JsonProperty("date_start")]
            public DateTime DateStart { get; set; }

            [JsonProperty("date_stop")]
            public DateTime DateStop { get; set; }

            [JsonProperty("actions")]
            public List<Action> Actions { get; set; }
        }

        public class InsightGeneral
        {
            [JsonProperty("reach")]
            public int Reach { get; set; }

            [JsonProperty("frequency")]
            public double Frequency { get; set; }

            [JsonProperty("ctr")]
            public double Ctr { get; set; }

            [JsonProperty("impressions")]
            public int Impressions { get; set; }

            [JsonProperty("cpc")]
            public double Cpc { get; set; }

            [JsonProperty("cpm")]
            public double Cpm { get; set; }

            [JsonProperty("spend")]
            public double Spend { get; set; }

            [JsonProperty("clicks")]
            public int Clicks { get; set; }

            [JsonProperty("date_start")]
            public DateTime DateStart { get; set; }

            [JsonProperty("date_stop")]
            public DateTime DateStop { get; set; }

            [JsonProperty("outbound_clicks")]
            public List<Action> OutboundClicks { get; set; }

            [JsonProperty("outbound_clicks_ctr")]
            public List<Action> OutboundClicksCtr { get; set; }

            [JsonProperty("actions")]
            public List<Action> Actions { get; set; }
        }

        public class ReportFilterResponse
        {
            [JsonProperty("data")]
            public List<ReportFilter> Data { get; set; }
        }

        public class ReportFilter
        {
            [JsonProperty("reach")]
            public int Reach { get; set; }

            [JsonProperty("frequency")]
            public double Frequency { get; set; }

            [JsonProperty("ctr")]
            public double Ctr { get; set; }

            [JsonProperty("impressions")]
            public int Impressions { get; set; }

            [JsonProperty("cpc")]
            public double Cpc { get; set; }

            [JsonProperty("cpm")]
            public double Cpm { get; set; }

            [JsonProperty("spend")]
            public double Spend { get; set; }

            [JsonProperty("clicks")]
            public int Clicks { get; set; }

            [JsonProperty("date_start")]
            public DateTime DateStart { get; set; }

            [JsonProperty("date_stop")]
            public DateTime DateStop { get; set; }

            [JsonProperty("ad_format_asset")]
            public string AdFormatAsset { get; set; }

            [JsonProperty("age")]
            public string Age { get; set; }

            [JsonProperty("app_id")]
            public string AppId { get; set; }

            [JsonProperty("body_asset")]
            public string BodyAsset { get; set; }

            [JsonProperty("breakdown_reporting_ad_id")]
            public string BreakdownReportingAdId { get; set; }

            [JsonProperty("call_to_action_asset")]
            public string CallToActionAsset { get; set; }

            [JsonProperty("coarse_conversion_value")]
            public string CoarseConversionValue { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("description_asset")]
            public string DescriptionAsset { get; set; }

            [JsonProperty("fidelity_type")]
            public string FidelityType { get; set; }

            [JsonProperty("gender")]
            public string Gender { get; set; }

            [JsonProperty("hsid")]
            public string Hsid { get; set; }

            [JsonProperty("image_asset")]
            public string ImageAsset { get; set; }

            [JsonProperty("impression_device")]
            public string ImpressionDevice { get; set; }

            [JsonProperty("is_conversion_id_modeled")]
            public string IsConversionIdModeled { get; set; }

            [JsonProperty("is_rendered_as_delayed_skip_ad")]
            public string IsRenderedAsDelayedSkipAd { get; set; }

            [JsonProperty("landing_destination")]
            public string LandingDestination { get; set; }

            [JsonProperty("link_url_asset")]
            public string LinkUrlAsset { get; set; }

            [JsonProperty("mdsa_landing_destination")]
            public string MdsaLandingDestination { get; set; }

            [JsonProperty("media_asset_url")]
            public string MediaAssetUrl { get; set; }

            [JsonProperty("media_creator")]
            public string MediaCreator { get; set; }

            [JsonProperty("media_destination_url")]
            public string MediaDestinationUrl { get; set; }

            [JsonProperty("media_format")]
            public string MediaFormat { get; set; }

            [JsonProperty("media_origin_url")]
            public string MediaOriginUrl { get; set; }

            [JsonProperty("media_text_content")]
            public string MediaTextContent { get; set; }

            [JsonProperty("media_type")]
            public string MediaType { get; set; }

            [JsonProperty("postback_sequence_index")]
            public string PostbackSequenceIndex { get; set; }

            [JsonProperty("product_id")]
            public string ProductId { get; set; }

            [JsonProperty("redownload")]
            public string Redownload { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }

            [JsonProperty("skan_campaign_id")]
            public string SkanCampaignId { get; set; }

            [JsonProperty("skan_conversion_id")]
            public string SkanConversionId { get; set; }

            [JsonProperty("skan_version")]
            public string SkanVersion { get; set; }

            [JsonProperty("sot_attribution_model_type")]
            public string SotAttributionModelType { get; set; }

            [JsonProperty("sot_attribution_window")]
            public string SotAttributionWindow { get; set; }

            [JsonProperty("sot_channel")]
            public string SotChannel { get; set; }

            [JsonProperty("sot_event_type")]
            public string SotEventType { get; set; }

            [JsonProperty("sot_source")]
            public string SotSource { get; set; }

            [JsonProperty("title_asset")]
            public string TitleAsset { get; set; }

            [JsonProperty("user_persona_id")]
            public string UserPersonaId { get; set; }

            [JsonProperty("user_persona_name")]
            public string UserPersonaName { get; set; }

            [JsonProperty("video_asset")]
            public string VideoAsset { get; set; }

            [JsonProperty("dma")]
            public string Dma { get; set; }

            [JsonProperty("frequency_value")]
            public string FrequencyValue { get; set; }

            [JsonProperty("hourly_stats_aggregated_by_advertiser_time_zone")]
            public string HourlyStatsAggregatedByAdvertiserTimeZone { get; set; }

            [JsonProperty("hourly_stats_aggregated_by_audience_time_zone")]
            public string HourlyStatsAggregatedByAudienceTimeZone { get; set; }

            [JsonProperty("mmm")]
            public string Mmm { get; set; }

            [JsonProperty("place_page_id")]
            public string PlacePageId { get; set; }

            [JsonProperty("publisher_platform")]
            public string PublisherPlatform { get; set; }

            [JsonProperty("platform_position")]
            public string PlatformPosition { get; set; }

            [JsonProperty("device_platform")]
            public string DevicePlatform { get; set; }

            [JsonProperty("standard_event_content_type")]
            public string StandardEventContentType { get; set; }

            [JsonProperty("conversion_destination")]
            public string ConversionDestination { get; set; }

            [JsonProperty("signal_source_bucket")]
            public string SignalSourceBucket { get; set; }

            [JsonProperty("marketing_messages_btn_name")]
            public string MarketingMessagesBtnName { get; set; }
        }

        public class Action
        {
            [JsonProperty("action_type")]
            public string ActionType { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }
        }

		public class MetaSummaryResponse
		{
			[JsonProperty("totalMeta")]
			public MetaMetrics TotalMeta { get; set; }
		}

		public class MetaMetrics
		{
			[JsonProperty("spend")]
			public double Spend { get; set; }

			[JsonProperty("impressions")]
			public double Impressions { get; set; }

			[JsonProperty("clicks")]
			public double Clicks { get; set; }
		}

		public class TopAds
		{
			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("image_url")]
			public string Url { get; set; }
		}

		#region Audience
		public class LookalikeSpec
        {
            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("origin")]
            public List<Origin> Origin { get; set; }

            [JsonProperty("ratio")]
            public double Ratio { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public class Origin
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public class Audience
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("approximate_count_upper_bound")]
            public int ApproximateCountUpperBound { get; set; }

            [JsonProperty("approximate_count_lower_bound")]
            public int ApproximateCountLowerBound { get; set; }

            [JsonProperty("lookalike_spec")]
            public LookalikeSpec LookalikeSpec { get; set; }

            [JsonProperty("time_created")]
            public long TimeCreatedUnix { get; set; }

            [JsonProperty("time_updated")]
            public long TimeUpdatedUnix { get; set; }

            [JsonProperty("audienceType")]
            public string AudienceType { get; set; }

            public DateTime TimeCreated => DateTimeOffset.FromUnixTimeSeconds(TimeCreatedUnix).DateTime;
            public DateTime TimeUpdated => DateTimeOffset.FromUnixTimeSeconds(TimeUpdatedUnix).DateTime;
        }
        #endregion
        #region SavedAudience
        public class Targeting
        {
            [JsonProperty("age_max")]
            public int AgeMax { get; set; }

            [JsonProperty("age_min")]
            public int AgeMin { get; set; }

            [JsonProperty("age_range")]
            public List<int> AgeRange { get; set; }

            [JsonProperty("flexible_spec")]
            public List<FlexibleSpec> FlexibleSpec { get; set; }

            [JsonProperty("genders")]
            public List<int> Genders { get; set; }

            [JsonProperty("geo_locations")]
            public GeoLocations GeoLocations { get; set; }

            [JsonProperty("targeting_automation")]
            public TargetingAutomation TargetingAutomation { get; set; }

            [JsonProperty("custom_audiences")]
            public List<CustomAudience> CustomAudiences { get; set; }
        }

        public class FlexibleSpec
        {
            [JsonProperty("interests")]
            public List<Interest> Interests { get; set; }
        }

        public class Interest
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class GeoLocations
        {
            [JsonProperty("cities")]
            public List<City> Cities { get; set; }

            [JsonProperty("location_types")]
            public List<string> LocationTypes { get; set; }
        }

        public class City
        {
            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("distance_unit")]
            public string DistanceUnit { get; set; }

            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("radius")]
            public int Radius { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }

            [JsonProperty("region_id")]
            public string RegionId { get; set; }
        }

        public class TargetingAutomation
        {
            [JsonProperty("advantage_audience")]
            public int AdvantageAudience { get; set; }
        }

        public class CustomAudience
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class SavedAudience
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("approximate_count_upper_bound")]
            public int ApproximateCountUpperBound { get; set; }

            [JsonProperty("approximate_count_lower_bound")]
            public int ApproximateCountLowerBound { get; set; }

            [JsonProperty("time_created")]
            public string TimeCreated { get; set; }

            [JsonProperty("time_updated")]
            public string TimeUpdated { get; set; }

            [JsonProperty("targeting")]
            public Targeting Targeting { get; set; }

            [JsonProperty("audienceType")]
            public string AudienceType { get; set; }
        }
        #endregion
        #endregion
    }
}
