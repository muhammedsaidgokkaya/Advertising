using AdminPanel.Models.Google.SearchConsole.Query;
using AdminPanel.Models.Google.SearchConsole.Site;
using AdminPanel.Models.Google.SearchConsole.SiteMap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google.SearchConsole
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SearchConsoleController : ControllerBase
    {
        private readonly ILogger<SearchConsoleController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public SearchConsoleController(ILogger<SearchConsoleController> logger, GoogleService googleService, GoogleData googleData)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService, googleData);
            _googleData = googleData;
            _defaultValues = new DefaultValues();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-sites")]
        public async Task<ActionResult<IEnumerable<SiteResponse>>> GetSites()
        {
            var userId = UserId();
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var sites = await _googleData.GetSiteDataAsync(accessTokenControl);

            var data = new SiteResponse
            {
                SiteEntry = sites.SiteEntry?.Select(q => new Sites
                {
                    SiteUrl = q.SiteUrl,
                    PermissionLevel = q.PermissionLevel
                }).ToList() ?? new List<Sites>()
            };

            return Ok(new List<SiteResponse> { data });
        }

        [HttpGet("search-console-account")]
        public ActionResult<IEnumerable<object>> GetOrganizationSearchConsoleAccount()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            var searchConsoleAccount = organization.GoogleSearchConsole;
            var result = searchConsoleAccount
                .Split(',')
                .Select(url => new { account = url.Trim(), accountId = url.Trim() })
                .ToList();

            return Ok(result);
        }

        [HttpGet("get-search-console-querys")]
        public async Task<ActionResult<IEnumerable<Row>>> GetSearchConsoleQuery(string accountId, string dimensions, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var rows = "5000";
            var searchConsoleQuery = await _googleData.GetSearchConsoleDataAsync(accessTokenControl, accountId, rows, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var data = searchConsoleQuery
                .Select((r, index) => new Row
                {
                    Id = index + 1,
                    Keys = r.Keys,
                    Clicks = r.Clicks,
                    Impressions = r.Impressions,
                    Ctr = r.Ctr,
                    Position = r.Position,
                })
                .ToList();

            return Ok(data);
        }

        [HttpGet("get-search-console")]
        public ActionResult<IEnumerable<SearchConsoleDashboard>> GetSearchConsole(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var searchConsoleQuery = _googleData.SearchConsoleAdmin(accessTokenControl, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var data = new SearchConsoleDashboard
            {
                TotalClicks = searchConsoleQuery.TotalClicks,
                TotalImpressions = searchConsoleQuery.TotalImpressions,
                AverageCtr = searchConsoleQuery.AverageCtr,
                AveragePosition = searchConsoleQuery.AveragePosition,
                ClicksChange = searchConsoleQuery.ClicksChange,
                ImpressionsChange = searchConsoleQuery.ImpressionsChange,
                CtrChange = searchConsoleQuery.CtrChange,
                PositionChange = searchConsoleQuery.PositionChange,
            };

            return Ok(new List<SearchConsoleDashboard> { data });
        }

        [HttpGet("get-search-console-chart-ten")]
        public async Task<ActionResult<IEnumerable<Row>>> GetSearchConsoleChartTen(string accountId, string dimensions, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var rows = "5000";
            var searchConsoleQuery = await _googleData.GetSearchConsoleDataAsync(accessTokenControl, accountId, rows, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var data = searchConsoleQuery.Select(r => new Row
            {
                Keys = r.Keys,
                Clicks = r.Clicks,
                Impressions = r.Impressions,
            }).Take(10).ToList();

            if (dimensions == "date")
            {
                data = searchConsoleQuery.Select(r => new Row
                {
                    Keys = r.Keys,
                    Clicks = r.Clicks,
                    Impressions = r.Impressions,
                }).OrderByDescending(d => d.Keys).Take(10).ToList();
            }

            return Ok(data);
        }

        [HttpGet("get-search-console-chart-four")]
        public async Task<ActionResult<IEnumerable<Row>>> GetSearchConsoleChartFour(string accountId, string dimensions, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var rows = "5000";
            var searchConsoleQuery = await _googleData.GetSearchConsoleDataAsync(accessTokenControl, accountId, rows, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var data = searchConsoleQuery.Select(r => new Row
            {
                Keys = r.Keys,
                Clicks = r.Clicks,
                Impressions = r.Impressions,
            }).OrderBy(d => d.Keys).Take(4).ToList();

            if (dimensions == "date")
            {
                data = searchConsoleQuery.Select(r => new Row
                {
                    Keys = r.Keys,
                    Clicks = r.Clicks,
                    Impressions = r.Impressions,
                }).OrderByDescending(d => d.Keys).Take(4).ToList();
            }

            return Ok(data);
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
