using Core.Domain.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleCallBackController : ControllerBase
    {
        private readonly ILogger<GoogleCallBackController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleData _googleData;

        public GoogleCallBackController(ILogger<GoogleCallBackController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleData = new GoogleData();
        }

        [HttpGet]
        public ActionResult<AdminPanel.Models.Google.AccessToken.GoogleAccesToken> Get(string code, string scope)
        {
            var userId = 1;
            var model = _googleService.GetGoogleApp(userId);
            var accessToken = _googleData.AccessTokenAdmin(model.AppId, model.AppSecret, model.RedirectUrl, code);
            var googleAccessToken = _googleService.AddGoogleAccessToken(model.Id, userId, accessToken.AccessToken, accessToken.RefreshToken, accessToken.ExpiresIn, accessToken.Scope, accessToken.TokenType);

            var google = new AdminPanel.Models.Google.AccessToken.GoogleAccesToken
            {
                AccessToken = accessToken.AccessToken,
                IsSuccess = googleAccessToken != 0
            };

            return Ok(google);
        }
    }
}
