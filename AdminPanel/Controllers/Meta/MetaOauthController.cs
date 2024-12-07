using AdminPanel.Models.Meta.AccessToken;
using Core.Domain.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.MetaData;

namespace AdminPanel.Controllers.Meta
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MetaOauthController : ControllerBase
    {
        private readonly ILogger<MetaOauthController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;
        private readonly MetaData _metaData;

        public MetaOauthController(ILogger<MetaOauthController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
            _metaData = new MetaData();
        }

        // https://www.facebook.com/v17.0/dialog/oauth?client_id=587025860502952&redirect_uri=https://localhost:7081/api/MetaOauth/call-back&scope=ads_read,ads_management,business_management,read_insights,pages_manage_ads&response_type=token
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("call-back")]
        public ActionResult<IEnumerable<MetaAccessToken>> Get(string access_token, string data_access_expiration_time, string expires_in)
        {
            var userId = UserId();
            var app = _metaService.GetMetaApp();
            var accessToken = _metaData.LongAccessTokenAdmin(app.AppId, app.AppSecret, access_token);
            var metaAccessToken = _metaService.AddLongAccessToken(app.Id, userId, accessToken.AccessToken, accessToken.TokenType, accessToken.ExpiresIn);

            var meta = new MetaAccessToken
            {
                AccessToken = accessToken.AccessToken,
                IsSuccess = metaAccessToken != 0
            };

            return Ok(meta);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("get-meta-app")]
        public ActionResult<MetaApp> GetGoogleApp()
        {
            var userId = UserId();
            var model = _metaService.GetMetaApp();
            var metaApp = new MetaApp
            {
                AppId = model.AppId,
                AppSecret = model.AppSecret,
            };

            return Ok(metaApp);
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
