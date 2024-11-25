using AdminPanel.Models.Google.SearchConsole.Site;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleSitesController : ControllerBase
    {
        private readonly ILogger<GoogleSitesController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;

        public GoogleSitesController(ILogger<GoogleSitesController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SiteResponse>> GetSites(int userId)
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
            var sites = googleData.SiteAdmin(control.AccessToken);

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
    }
}
