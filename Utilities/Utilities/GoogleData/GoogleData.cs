using Google.Apis.Auth.OAuth2;
using Google.Apis.SearchConsole.v1.Data;
using Google.Apis.SearchConsole.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Utilities.Helper;
using Utilities.Utilities.GoogleData.SearchConsole;
using static Utilities.Utilities.GoogleData.GoogleData;

namespace Utilities.Utilities.GoogleData
{
    public class GoogleData
    {
        private readonly PythonRun _pythonRun;
        private readonly IConfiguration _configuration;
        private readonly Utilities.GoogleData.SearchConsole.SearchConsole _searchConsole;
        private readonly Utilities.GoogleData.Analytics.Analytics _analyticsConsole;

        public GoogleData(IConfiguration configuration)
        {
            _pythonRun = new PythonRun();
            _configuration = configuration;
			_searchConsole = new Utilities.GoogleData.SearchConsole.SearchConsole();
			_analyticsConsole = new Utilities.GoogleData.Analytics.Analytics();
		}

        private string GetPythonScriptPath(string relativePath)
        {
            string basePath = _configuration["PythonScriptBasePath"];
            return Path.Combine(basePath, relativePath);
        }

        #region Google
        public AccessTokenResponse AccessTokenAdmin(string client_id, string client_secret, string redirect_uri, string authorization_code)
        {
            string pythonScriptPath = GetPythonScriptPath("Google/AccessToken/accessTokenAdmin.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, client_id, client_secret, redirect_uri, authorization_code);

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

        public AccessTokenResponse RefreshAccessTokenAdmin(string client_id, string client_secret, string refresh_token)
        {
            string pythonScriptPath = GetPythonScriptPath("Google/AccessToken/refreshToken.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, client_id, client_secret, refresh_token);

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

        #region SearchConsole
		public async Task<SiteResponse> GetSiteDataAsync(string accessToken)
		{
			var url = "https://www.googleapis.com/webmasters/v3/sites";

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

				var response = await client.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();

					var siteResponse = JsonConvert.DeserializeObject<SiteResponse>(content);

					return siteResponse;
				}
				else
				{
					throw new Exception($"API çağrısı başarısız oldu. Durum Kodu: {response.StatusCode}");
				}
			}
		}

		public async Task<List<Row>> GetSearchConsoleDataAsync(string access_token, string site_url, string rows, string dimensions, string start_date, string end_date)
		{
			var credential = GoogleCredential.FromAccessToken(access_token);

			var service = new SearchConsoleService(new BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "Google Search Console API Sample",
			});

			var request = service.Searchanalytics.Query(new SearchAnalyticsQueryRequest
			{
				StartDate = start_date,
				EndDate = end_date,
				Dimensions = new List<string> { dimensions },
				RowLimit = 5000
			}, site_url);

			var response = await request.ExecuteAsync();

			var formattedResponse = new List<Row>();

			if (response.Rows != null)
			{
				foreach (var row in response.Rows)
				{
					formattedResponse.Add(new Row
					{
						Keys = row.Keys[0],
						Clicks = row.Clicks ?? 0.0,
						Impressions = row.Impressions ?? 0.0,
						Ctr = row.Ctr ?? 0.0,
						Position = row.Position ?? 0.0
					});
				}
			}

			return formattedResponse;
		}

		public SearchConsole SearchConsoleAdmin(string access_token, string site_url, string start_date, string end_date)
		{
			var metrics = _searchConsole.GetSearchConsoleMetrics(access_token, site_url, start_date, end_date);

			var result = new SearchConsole
			{
				TotalClicks = metrics.TotalClicks,
				TotalImpressions = metrics.TotalImpressions,
				AverageCtr = metrics.AverageCtr,
				AveragePosition = metrics.AveragePosition,
				ClicksChange = metrics.ClicksChange,
				ImpressionsChange = metrics.ImpressionsChange,
				CtrChange = metrics.CtrChange,
				PositionChange = metrics.PositionChange
			};

			return result;
		}

		public SearchConsole SearchConsoleDashboardAdmin(string access_token, List<string> accountIds)
		{
			var metrics = _searchConsole.GetAllSearchConsoleMetrics(access_token, accountIds);

			var result = new SearchConsole
			{
				TotalClicks = metrics.totalClicksSum,
				TotalImpressions = metrics.totalImpressionsSum,
			};

			return result;
		}
		#endregion

		#region Analytics

		public AccountSummaryResponse AccountSummaryAdmin(string access_token)
        {
			try
			{
				var result = _analyticsConsole.AccountSummaryAdmin(access_token);
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Google Analytics verisi alınırken hata oluştu.", ex);
			}
		}

		public async Task<List<DashboardResponse>> DashboardAdmin(string access_token, string property_id, string start_date, string end_date)
		{
			try
			{
				var result = await _analyticsConsole.GetAnalyticsDashboardMonthly(access_token, property_id, start_date, end_date);
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Google Analytics verisi alınırken hata oluştu.", ex);
			}
		}

		public async Task<int> Dashboards(string access_token, List<string> accountIds)
		{
			try
			{
				var result = await _analyticsConsole.GetTotalActiveUsers(access_token, accountIds);
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Google Analytics verisi alınırken hata oluştu.", ex);
			}
		}

		public async Task<List<DashboardDimensionResponse>> DashboardDimensionAdmin(string access_token, string property_id, string dimension, string metric, string start_date, string end_date)
        {
			try
			{
				var result = await _analyticsConsole.GetAnalyticsDimensionData(access_token, property_id, dimension, metric, start_date, end_date);
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Google Analytics verisi alınırken hata oluştu.", ex);
			}
		}

        public async Task<List<GeneralCountResponse>> GeneralCountAdmin(string access_token, string property_id, string dimension, string start_date, string end_date)
        {
			try
			{
				var result = await _analyticsConsole.GetAnalyticsGeneralCount(access_token, property_id, dimension, start_date, end_date);
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Google Analytics verisi alınırken hata oluştu.", ex);
			}
		}

        public async Task<List<GeneralRateResponse>> GeneralRateAdmin(string access_token, string property_id, string dimension, string start_date, string end_date)
        {
			try
			{
				var result = await _analyticsConsole.GetAnalyticsGeneralRate(access_token, property_id, dimension, start_date, end_date);
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Google Analytics verisi alınırken hata oluştu.", ex);
			}
		}
        #endregion

        #endregion

        #region Class
        public class AccessTokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }

        #region SearchConsole
        public class SiteResponse
        {
            [JsonProperty("siteEntry")]
            public List<Sites> SiteEntry { get; set; }
        }

        public class Sites
        {
            [JsonProperty("siteUrl")]
            public string SiteUrl { get; set; }

            [JsonProperty("permissionLevel")]
            public string PermissionLevel { get; set; }
        }

        public class Sitemap
        {
            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("lastSubmitted")]
            public DateTime LastSubmitted { get; set; }

            [JsonProperty("isPending")]
            public bool IsPending { get; set; }

            [JsonProperty("isSitemapsIndex")]
            public bool IsSitemapsIndex { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("lastDownloaded")]
            public DateTime LastDownloaded { get; set; }

            [JsonProperty("warnings")]
            public int Warnings { get; set; }

            [JsonProperty("errors")]
            public int Errors { get; set; }

            [JsonProperty("contents")]
            public List<Content> Contents { get; set; }
        }

        public class Content
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("submitted")]
            public int Submitted { get; set; }

            [JsonProperty("indexed")]
            public int Indexed { get; set; }
        }

        public class SitemapResponse
        {
            [JsonProperty("sitemap")]
            public List<Sitemap> Sitemap { get; set; }
        }

        public class Row
        {
            [JsonProperty("keys")]
            public string Keys { get; set; }

            [JsonProperty("clicks")]
            public double Clicks { get; set; }

            [JsonProperty("impressions")]
            public double Impressions { get; set; }

            [JsonProperty("ctr")]
            public double Ctr { get; set; }

            [JsonProperty("position")]
            public double Position { get; set; }
        }

        public class RowResponse
        {
            [JsonProperty("rows")]
            public List<Row> Rows { get; set; }

            [JsonProperty("responseAggregationType")]
            public string ResponseAggregationType { get; set; }
        }

        public class SearchConsole
        {
            [JsonProperty("total_clicks")]
            public int TotalClicks { get; set; }

            [JsonProperty("total_impressions")]
            public int TotalImpressions { get; set; }

            [JsonProperty("average_ctr")]
            public double AverageCtr { get; set; }

            [JsonProperty("average_position")]
            public double AveragePosition { get; set; }

            [JsonProperty("clicks_change")]
            public double ClicksChange { get; set; }

            [JsonProperty("impressions_change")]
            public double ImpressionsChange { get; set; }

            [JsonProperty("ctr_change")]
            public double CtrChange { get; set; }

            [JsonProperty("position_change")]
            public double PositionChange { get; set; }
        }
        #endregion

        #region Analytics
        public class AccountSummary
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("propertySummaries")]
            public List<PropertySummary> PropertySummaries { get; set; }
        }

        public class PropertySummary
        {
            [JsonProperty("property")]
            public string Property { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("propertyType")]
            public string PropertyType { get; set; }

            [JsonProperty("parent")]
            public string Parent { get; set; }
        }

        public class AccountSummaryResponse
        {
            [JsonProperty("accountSummaries")]
            public List<AccountSummary> AccountSummaries { get; set; }
        }

        public class DashboardResponse
        {
            [JsonProperty("month")]
            public string Month { get; set; }

            [JsonProperty("data")]
            public List<Dashboard> Data { get; set; }
        }

        public class Dashboard
        {
            [JsonProperty("activeUsers")]
            public int ActiveUsers { get; set; }

            [JsonProperty("eventCount")]
            public int EventCount { get; set; }

            [JsonProperty("newUsers")]
            public int NewUsers { get; set; }

            [JsonProperty("engagedSessions")]
            public int EngagedSessions { get; set; }
        }

        public class DashboardDimensionResponse
        {
            [JsonProperty("dimension")]
            public string Dimension { get; set; }

            [JsonProperty("metric")]
            public double Metric { get; set; }
        }

        public class GeneralCountResponse
        {
            [JsonProperty("dimension")]
            public string Dimension { get; set; }

            [JsonProperty("totalUsers")]
            public int TotalUsers { get; set; }

            [JsonProperty("activeUsers")]
            public int ActiveUsers { get; set; }

            [JsonProperty("newUsers")]
            public int NewUsers { get; set; }

            [JsonProperty("screenPageViews")]
            public int ScreenPageViews { get; set; }

            [JsonProperty("sessions")]
            public int Sessions { get; set; }

            [JsonProperty("eventCount")]
            public int EventCount { get; set; }

            [JsonProperty("keyEvents")]
            public int KeyEvents { get; set; }

            [JsonProperty("totalRevenue")]
            public double TotalRevenue { get; set; }

            [JsonProperty("transactions")]
            public int Transactions { get; set; }
        }

        public class GeneralRateResponse
        {
            [JsonProperty("dimension")]
            public string Dimension { get; set; }

            [JsonProperty("averageSessionDuration")]
            public double AverageSessionDuration { get; set; }

            [JsonProperty("eventsPerSession")]
            public double EventsPerSession { get; set; }

            [JsonProperty("sessionKeyEventRate")]
            public double SessionKeyEventRate { get; set; }

            [JsonProperty("screenPageViewsPerSession")]
            public double ScreenPageViewsPerSession { get; set; }

            [JsonProperty("engagementRate")]
            public double EngagementRate { get; set; }

            [JsonProperty("engagedSessions")]
            public int EngagedSessions { get; set; }

            [JsonProperty("screenPageViewsPerUser")]
            public double ScreenPageViewsPerUser { get; set; }

            [JsonProperty("eventCountPerUser")]
            public double EventCountPerUser { get; set; }

            [JsonProperty("userKeyEventRate")]
            public double UserKeyEventRate { get; set; }
        }
        #endregion

        #endregion
    }
}
