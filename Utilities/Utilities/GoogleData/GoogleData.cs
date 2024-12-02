using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Utilities.Helper;
using static Utilities.Utilities.GoogleData.GoogleData;

namespace Utilities.Utilities.GoogleData
{
    public class GoogleData
    {
        private readonly PythonRun _pythonRun;

        public GoogleData()
        {
            _pythonRun = new PythonRun();
        }

        #region Google
        public AccessTokenResponse AccessTokenAdmin(string client_id, string client_secret, string redirect_uri, string authorization_code)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "AccessToken", "accessTokenAdmin.py");
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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "AccessToken", "refreshToken.py");
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
        public SiteResponse SiteAdmin(string access_token)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "SearchConsole", "sites.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<SiteResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public SitemapResponse SiteMapAdmin(string access_token, string site_url)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "SearchConsole", "siteMap.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, site_url);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<SitemapResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public RowResponse SearchConsoleQueryAdmin(string access_token, string site_url, string rows, string dimensions, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "SearchConsole", "searchConsoleQuery.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, site_url, rows, dimensions, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<RowResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }
        #endregion

        #region Analytics
        public AccountSummaryResponse AccountSummaryAdmin(string access_token)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "Analytics", "accountSummary.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<AccountSummaryResponse>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public List<DashboardResponse> DashboardAdmin(string access_token, string property_id, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "Analytics", "analyticsDashboard.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, property_id, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<List<DashboardResponse>>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public List<DashboardDimensionResponse> DashboardDimensionAdmin(string access_token, string property_id, string dimension, string metric, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "Analytics", "analyticsDashboardDimension.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, property_id, dimension, metric, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<List<DashboardDimensionResponse>>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public List<GeneralCountResponse> GeneralCountAdmin(string access_token, string property_id, string dimension, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "Analytics", "analyticsGeneralCount.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, property_id, dimension, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<List<GeneralCountResponse>>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
            }
        }

        public List<GeneralRateResponse> GeneralRateAdmin(string access_token, string property_id, string dimension, string start_date, string end_date)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "Analytics", "analyticsGeneralRate.py");
            var jsonOutput = _pythonRun.RunPythonScript(pythonScriptPath, access_token, property_id, dimension, start_date, end_date);

            try
            {
                var tokenResponse = JsonConvert.DeserializeObject<List<GeneralRateResponse>>(jsonOutput.ToString());
                return tokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Hata.");
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
            public List<string> Keys { get; set; }

            [JsonProperty("clicks")]
            public int Clicks { get; set; }

            [JsonProperty("impressions")]
            public int Impressions { get; set; }

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
