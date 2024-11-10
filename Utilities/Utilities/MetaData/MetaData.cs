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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "longAccessTokenAdmin.py");
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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "businessAdmin.py");
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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "advertisingAccountsAdmin.py");
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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "campaigns.py");
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
            string pythonScriptPath = Path.Combine("C:", "Users", "furka", "Desktop", "Advertising", "Utilities", "Scripts", "Meta", "adsets.py");
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
            public string EndTime { get; set; }
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
            public string DailyBudget { get; set; }

            [JsonProperty("lifetime_budget")]
            public string LifeTimeBudget { get; set; }

            [JsonProperty("updated_time")]
            public string UpdateTime { get; set; }

            [JsonProperty("start_time")]
            public string StartTime { get; set; }

            [JsonProperty("end_time")]
            public string EndTime { get; set; }

            [JsonProperty("insights")]
            public InsightResponse Insights { get; set; }
        }

        public class InsightResponse
        {
            [JsonProperty("data")]
            public List<Insight> Data { get; set; }
        }

        public class Insight
        {
            [JsonProperty("reach")]
            public string Reach { get; set; }

            [JsonProperty("impressions")]
            public string Impressions { get; set; }

            [JsonProperty("cpc")]
            public string Cpc { get; set; }

            [JsonProperty("cpm")]
            public string Cpm { get; set; }

            [JsonProperty("spend")]
            public string Spend { get; set; }

            [JsonProperty("date_start")]
            public string DateStart { get; set; }

            [JsonProperty("date_stop")]
            public string DateStop { get; set; }

            [JsonProperty("actions")]
            public List<Action> Actions { get; set; }
        }

        public class Action
        {
            [JsonProperty("action_type")]
            public string ActionType { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }
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
