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

        public SearchConsoleController(ILogger<SearchConsoleController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
            _defaultValues = new DefaultValues();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-sites")]
        public ActionResult<IEnumerable<SiteResponse>> GetSites()
        {
            var userId = UserId();
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var sites = _googleData.SiteAdmin(accessTokenControl);

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

        [Authorize(Roles = "Admin,Read")]
        [HttpGet("get-site-maps")]
        public ActionResult<IEnumerable<SitemapResponse>> GetSiteMaps(string url)
        {
            var userId = UserId();
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var siteMaps = _googleData.SiteMapAdmin(accessTokenControl, url);

            var data = new SitemapResponse
            {
                Sitemap = siteMaps.Sitemap?.Select(s => new Sitemap
                {
                    Path = s.Path,
                    LastSubmitted = s.LastSubmitted,
                    IsPending = s.IsPending,
                    IsSitemapsIndex = s.IsSitemapsIndex,
                    Type = s.Type,
                    LastDownloaded = s.LastDownloaded,
                    Warnings = s.Warnings,
                    Errors = s.Errors,
                    Contents = s.Contents?.Select(c => new Content
                    {
                        Type = c.Type,
                        Submitted = c.Submitted,
                        Indexed = c.Indexed
                    }).ToList() ?? new List<Content>()
                }).ToList() ?? new List<Sitemap>()
            };

            return Ok(new List<SitemapResponse> { data });
        }

        [HttpGet("get-search-console-querys")]
        public ActionResult<IEnumerable<RowResponse>> GetSearchConsoleQuery(string url, string dimensions, string rows = "10", DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var searchConsoleQuery = _googleData.SearchConsoleQueryAdmin(accessTokenControl, url, rows, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var data = new RowResponse
            {
                Rows = searchConsoleQuery.Rows?.Select(s => new Row
                {
                    Keys = s.Keys,
                    Clicks = s.Clicks,
                    Impressions = s.Impressions,
                    Ctr = s.Ctr,
                    Position = s.Position
                }).ToList() ?? new List<Row>(),
                ResponseAggregationType = searchConsoleQuery.ResponseAggregationType
            };

            return Ok(new List<RowResponse> { data });
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
