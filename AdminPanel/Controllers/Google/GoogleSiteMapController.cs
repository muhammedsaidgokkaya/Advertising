using AdminPanel.Models.Google.SearchConsole.SiteMap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleSiteMapController : ControllerBase
    {
        private readonly ILogger<GoogleSiteMapController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;

        public GoogleSiteMapController(ILogger<GoogleSiteMapController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SitemapResponse>> GetSiteMaps(int userId, string url)
        {
            var control = _googleService.GetGoogleAccessToken(userId);
            GoogleData googleData = new GoogleData();
            if (control == null)
            {
                var model = _googleService.GetGoogleApp(userId);
                var accessToken = _googleService.GetGoogleAccessTokenControl(userId);
                var refreshToken = googleData.RefreshAccessTokenAdmin(model.AppId, model.AppSecret, accessToken.RefreshToken);
                var newAccessToken = _googleService.AddGoogleAccessToken(model.Id, userId, refreshToken.AccessToken, accessToken.RefreshToken, refreshToken.ExpiresIn, refreshToken.Scope, refreshToken.TokenType);
                control = _googleService.GetGoogleAccessToken(userId);
            }
            var siteMaps = googleData.SiteMapAdmin(control.AccessToken, url);

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
