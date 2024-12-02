using AdminPanel.Models.Google.SearchConsole.SiteMap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google.SearchConsole
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleSiteMapController : ControllerBase
    {
        private readonly ILogger<GoogleSiteMapController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;

        public GoogleSiteMapController(ILogger<GoogleSiteMapController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
        }

        [HttpGet]
        public ActionResult<IEnumerable<SitemapResponse>> GetSiteMaps(int userId, string url)
        {
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
    }
}
