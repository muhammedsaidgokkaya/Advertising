using Newtonsoft.Json;
using System.Diagnostics;
using static Utilities.Utilities.GoogleData.GoogleData;

namespace Utilities.Utilities.GoogleData
{
    public class GoogleData
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

        #region Google
        public AccessTokenResponse AccessTokenAdmin(string client_id, string client_secret, string redirect_uri, string authorization_code)
        {
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "AccessToken", "accessTokenAdmin.py");
            var jsonOutput = RunPythonScript(pythonScriptPath, client_id, client_secret, redirect_uri, authorization_code);

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
            var jsonOutput = RunPythonScript(pythonScriptPath, client_id, client_secret, refresh_token);

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
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token);

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
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, site_url);

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
            var jsonOutput = RunPythonScript(pythonScriptPath, access_token, site_url, rows, dimensions, start_date, end_date);

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

        #endregion

        #region Test
        public object LongAccessToken(string client_id, string client_secret, string redirect_uri, string authorization_code)
        {
            try
            {
                string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Google", "AccessToken", "accessTokenAdmin.py");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScriptPath}\" {client_id} {client_secret} {redirect_uri} {authorization_code}",
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
