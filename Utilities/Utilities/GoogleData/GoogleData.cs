﻿using Newtonsoft.Json;
using System.Diagnostics;

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