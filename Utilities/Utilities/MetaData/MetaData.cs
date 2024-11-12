using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Utilities.Utilities.MetaData
{
    public class MetaData
    {
        public object RunPythonScript(string scriptPath, params string[] args)
        {
            try
            {
                // Argümanları birleştirerek Python komutuna ekliyoruz
                string arguments = $"\"{scriptPath}\" " + string.Join(" ", args);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return output;
                }
            }
            catch (Exception ex)
            {
                return new { error = ex.Message };
            }
        }

        #region Meta
        public AccessTokenResponse LongAccessTokenAdmin(string app_id, string app_secret, string short_lived_token)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "AccessToken", "longAccessTokenAdmin.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, app_id, app_secret, short_lived_token);

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

        public BusinessResponse BusinessAdmin(string access_token)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "Business", "businessAdmin.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<BusinessResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public AdvertisingAccountsResponse AdvertisingAccountsAdmin(string access_token, string business_id)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "Business", "advertisingAccountsAdmin.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, business_id);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<AdvertisingAccountsResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public CampaignResponse CampaignsAdmin(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "AdvertisingManager", "campaigns.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "AdvertisingManager", "adsets.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<AdSetResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public AdResponse AdsAdmin(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "AdvertisingManager", "ads.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<AdResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public InsightGeneralResponse InsightsAdmin(string access_token, string ad_account_id, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "Report", "insights.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date);

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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "Report", "insgihtsFilter.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, ad_account_id, start_date, end_date, filterParams);

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
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("configured_status")]
            public string Status { get; set; }

            [JsonProperty("adset")]
            public AdSet AdSet { get; set; }

            [JsonProperty("insights")]
            public InsightResponse Insights { get; set; }
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
        #endregion

        #region Test
        //Test içeriğidir
        public object deneme(string accessToken, string adAccountId)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "test.py");
            return RunPythonScript(pythonScriptPath, accessToken, adAccountId);
        }

        public object LongAccessToken(string app_id, string app_secret, string short_lived_token)
        {
            try
            {
                string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "longAccessTokenAdmin.py");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScriptPath}\" {app_id} {app_secret} {short_lived_token}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output;
            }
            catch (Exception ex)
            {
                return new { error = ex.Message };
            }
        }
        #endregion
    }
}
