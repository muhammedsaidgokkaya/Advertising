using AdminTest.Models;
using AdminTest.Models.Meta.Ad;
using AdminTest.Models.Meta.AdSet;
using AdminTest.Models.Meta.AdvertisingAccount;
using AdminTest.Models.Meta.Business;
using AdminTest.Models.Meta.Campaign;
using AdminTest.Models.Meta.Insight;
using AdminTest.Models.Meta.Report;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AdminTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        //public async Task<IActionResult> GetData()
        //{
        //    var url = "https://localhost:7081/api";

        //    var response = await _httpClient.GetStringAsync(url);

        //    var data = JsonConvert.DeserializeObject<List<MyModel>>(response);

        //    return View(data);
        //}
        public async Task<IActionResult> Index()
        {
            var url = "https://localhost:7081/api/MetaBusiness?userId=1";
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<BusinessResponse>>(response);
            return View(data);
        }

        public async Task<IActionResult> Advertising(string businessId)
        {
            var url = "https://localhost:7081/api/MetaAdvertisingAccount?userId=1&businessId=" + businessId;
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<AdvertisingAccountsResponse>>(response);
            return View(data);
        }

        public async Task<IActionResult> Ads(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "https://localhost:7081/api/MetaAd?userId=1&accountId=" + accountId + "&startDate=" + startDate + "&endDate=" + endDate;
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<AdResponse>>(response);
            return View(data);
        }

        public async Task<IActionResult> AdSets(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "https://localhost:7081/api/MetaAdSet?userId=1&accountId=" + accountId + "&startDate=" + startDate + "&endDate=" + endDate;
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<AdSetResponse>>(response);
            return View(data);
        }

        public async Task<IActionResult> Campaigns(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "https://localhost:7081/api/MetaCampaign?userId=1&accountId=" + accountId + "&startDate=" + startDate + "&endDate=" + endDate;
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<CampaignResponse>>(response);
            return View(data);
        }

        public async Task<IActionResult> Report(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "https://localhost:7081/api/Meta?userId=1&accountId=" + accountId + "&startDate=" + startDate + "&endDate=" + endDate;
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<InsightGeneralResponse>>(response);
            return View(data);
        }

        public async Task<IActionResult> ReportFilter(int userId, string accountId, string reportFilter = ",", DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "https://localhost:7081/api/MetaReport?userId=1&accountId=" + accountId + "&reportFilter=" + reportFilter + "&startDate=" + startDate + "&endDate=" + endDate;
            // testUrl = https://localhost:7043/Home/ReportFilter?accountId="act_342280538743641"&reportFilter="age,gender"
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<ReportFilterResponse>>(response);
            return View(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
