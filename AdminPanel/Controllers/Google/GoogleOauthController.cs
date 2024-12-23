using AdminPanel.Models.Google.AccessToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleOauthController : ControllerBase
    {
        private readonly ILogger<GoogleOauthController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleData _googleData;

        public GoogleOauthController(ILogger<GoogleOauthController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleData = new GoogleData();
        }

        // https://accounts.google.com/o/oauth2/auth?response_type=code&client_id=876265473668-dkrg8ouj2qaginhpoamfdacf0f83002j.apps.googleusercontent.com&redirect_uri=https://localhost:7081/api/GoogleOauth/call-back&scope=https://www.googleapis.com/auth/analytics.readonly%20https://www.googleapis.com/auth/analytics.manage.users.readonly%20https://www.googleapis.com/auth/webmasters&access_type=offline
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("call-back")]
        public ActionResult<AdminPanel.Models.Google.AccessToken.GoogleAccesToken> Get(string code, string scope)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var model = _googleService.GetGoogleApp();
            var accessToken = _googleData.AccessTokenAdmin(model.AppId, model.AppSecret, model.RedirectUrl, code);
            var googleAccessToken = _googleService.AddGoogleAccessToken(model.Id, user.OrganizationId, accessToken.AccessToken, accessToken.RefreshToken, accessToken.ExpiresIn, accessToken.Scope, accessToken.TokenType);

            var google = new AdminPanel.Models.Google.AccessToken.GoogleAccesToken
            {
                AccessToken = accessToken.AccessToken,
                IsSuccess = googleAccessToken != 0
            };

            return Ok(google);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("get-google-app")]
        public ActionResult<GoogleApp> GetGoogleApp()
        {
            var model = _googleService.GetGoogleApp();
            var googleApp = new GoogleApp
            {
                AppId = model.AppId,
                AppSecret = model.AppSecret,
                RedirectUrl = model.RedirectUrl
            };

            return Ok(googleApp);
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
