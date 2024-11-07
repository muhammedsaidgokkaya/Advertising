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
