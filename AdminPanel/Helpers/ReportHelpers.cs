using AdminPanel.Models.Report.OpenAI;
using Service.Implementations.Report;
using System.Text.RegularExpressions;

namespace AdminPanel.Helpers
{
    public class ReportHelpers
    {
        private readonly IConfiguration _configuration;
        private readonly ReportService _reportService;

        public ReportHelpers(IConfiguration configuration)
        {
            _configuration = configuration;
            _reportService = new ReportService();
        }

        public async Task<int> GeneralReportAI(string name, string account, string accountId, string typeId, int reportType, int organizationId, string prompt, DateTime? startDate = null, DateTime? endDate = null)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                var assistantResponse = result.Choices[0].Message.Content;
                assistantResponse = Regex.Replace(assistantResponse, @"\*\*(.*?)\*\*", "<b>$1</b>");
                assistantResponse = Regex.Replace(assistantResponse, @"^##\s*", "", RegexOptions.Multiline);
                var newReport = _reportService.AddReport(name, account, accountId, typeId, assistantResponse, reportType, organizationId, startDate, endDate);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public async Task<int> GeneralReportChartAI(string name, string account, string accountId, string typeId, int reportType, int organizationId, string prompt, DateTime? startDate = null, DateTime? endDate = null)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                var assistantResponse = result.Choices[0].Message.Content;
                var newReport = _reportService.AddReport(name, account, accountId, typeId, assistantResponse, reportType, organizationId, startDate, endDate);
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
