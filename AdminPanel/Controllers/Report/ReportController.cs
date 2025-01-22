using AdminPanel.Controllers.Organization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI.API.Completions;
using OpenAI.API;
using Service.Implementations.User;
using Utilities.Helper;
using AdminPanel.Models.Organization.User;
using Microsoft.AspNetCore.Authorization;
using AdminPanel.Models.Report;
using Service.Implementations.Report;
using Core.Domain.User;
using System.Security.Principal;
using System;
using Utilities.Utilities.GoogleData;
using Service.Implementations.Google;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using AdminPanel.Models.Report.OpenAI;
using System.Xml.Linq;
using AdminPanel.Helpers;

namespace AdminPanel.Controllers.Report
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly ReportService _reportService;
        private readonly ReportHelpers _reportHelpers;
        private readonly DefaultValues _defaultValues;
        private readonly EmailHelper _emailHelper;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;

        public ReportController(ILogger<ReportController> logger, IConfiguration configuration, GoogleService googleService, GoogleData googleData)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = new UserService();
            _reportService = new ReportService();
            _reportHelpers = new ReportHelpers(configuration);
            _defaultValues = new DefaultValues();
            _emailHelper = new EmailHelper();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService, googleData);
            _googleData = googleData;
        }

        [HttpGet("reports")]
        public ActionResult<IEnumerable<GetReports>> GetReports(string accountId, int reportType, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 7);
            var reports = _reportService.GetReports(user.OrganizationId, accountId, reportType, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd")).OrderByDescending(report => report.InsertedDate); ;
            var reportList = reports.Select(report => new GetReports
            {
                Id = report.Id,
                Name = report.Name,
                Account = report.Account,
                TypeId = report.TypeId,
                InsertedDate = report.InsertedDate,
            }).ToList();

            return Ok(reportList);
        }

        [HttpGet("report")]
        public ActionResult<IEnumerable<GetReport>> GetReport(int id, int reportType)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var report = _reportService.GetReport(id, user.OrganizationId, reportType);
            var data = new GetReport
            {
                Name = report.Name,
                Account = report.Account,
                TypeId = report.TypeId,
                Content = report.Content,
                StartDate = report.StartDate,
                EndDate = report.EndDate,
            };

            return Ok(data);
        }

        [HttpPost("delete-report")]
        public IActionResult DeleteReport(int id)
        {
            var report = _reportService.IsDeletedReport(id);
            if (report == 0)
            {
                return Ok(new { success = false });
            }
            return Ok(new { success = true });
        }

        [HttpPost]
        [Route("chatgpt-prompt-search-console")]
        public async Task<IActionResult> GeneratePromptSearchConsole(string name, string account, string accountId, string typeId, int reportType, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            object searchConsoleQuery = null;

            if (typeId == "Genel")
            {
                searchConsoleQuery = _googleData.SearchConsoleAdmin(
                    accessTokenControl,
                    accountId,
                    defaultValues[0].ToString("yyyy-MM-dd"),
                    defaultValues[1].ToString("yyyy-MM-dd")
                );
            }
            else
            {
                var queryType = new Dictionary<string, string>
                {
                    { "Sorgu", "query" },
                    { "Sayfa Sayısı", "page" },
                    { "Ülke", "country" },
                    { "Cihaz", "device" },
                    { "Arama Görünümü", "searchAppearance" },
                    { "Tarih", "date" }
                };

                if (queryType.ContainsKey(typeId))
                {
                    var typeValue = queryType[typeId];
                    searchConsoleQuery = _googleData.SearchConsoleQueryAdmin(
                        accessTokenControl,
                        accountId,
                        "50",
                        typeValue,
                        defaultValues[0].ToString("yyyy-MM-dd"),
                        defaultValues[1].ToString("yyyy-MM-dd")
                    );
                }
            }
            string jsonData = JsonConvert.SerializeObject(searchConsoleQuery);
            string prompt = $"Kullanıcıya React projemde görüntüleyebileceği formatta numaralandırarak aşağıdaki veriye dayanarak detaylı bir rapor, performans analizi ve geliştirme önerileri oluştur.\r\n Veri: {jsonData}\r\n Veriyi analiz ederken şu kurallara uymalısın:\r\n\r\n1. **Kalın Yazılar:** `**` işaretlerini HTML `<b></b>` formatında kalın yazıya dönüştür.\r\n2. **Başlıkları Kaldır:** `##` gibi Markdown başlık işaretlerini kaldır. Ancak içerik düzenini koru.\r\n3. **Numaralandırılmış Liste:** Sonuçları numaralandırılmış şekilde ver.\r\n4. **Detay Seviyeleri:** Analizde ilk 3 madde detaylı, diğerleri kısa ve öz olsun.\r\n\r\nVeri: \r\n{{jsonData}}\r\n\r\nYanıtı şu formatta döndür:\r\n1. Genel Performans:\r\n2. Hesap Verileri Analizi (ilk 3 tanesi detaylı diğerleri tek cümle olacak şekilde):\r\n3. Genel Öneriler (en detaylı olacak kısım):\r\nBu 3 başlık dışında hiçbir şey yazma. Sadece ve sadece 3 başlığı doldur.";
            var reportResult = await _reportHelpers.GeneralReportAI(name, account, accountId, typeId, reportType, user.OrganizationId, prompt, defaultValues[0].ToUniversalTime(), defaultValues[1].ToUniversalTime());

            if (reportResult == 1)
            {
                return Ok(new { success = true, message = "Rapor başarıyla oluşturuldu." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Rapor oluşturulamadı." });
            }
        }

        private int UserId()
        {
            var userIdClaim = HttpContext.User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return 0;
            }

            int userId = int.Parse(userIdClaim.Value);
            return userId;
        }
    }
}
