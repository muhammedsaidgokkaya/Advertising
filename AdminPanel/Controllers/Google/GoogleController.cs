using AdminPanel.Models.Google.AccessToken;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;

namespace AdminPanel.Controllers.Google
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly ILogger<GoogleController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;

        public GoogleController(ILogger<GoogleController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
        }

        [HttpGet]
        public ActionResult<GoogleApp> GetGoogleApp(int userId) 
        {
            var model = _googleService.GetGoogleApp(userId);
            var googleApp = new GoogleApp
            {
                AppId = model.AppId,
                AppSecret = model.AppSecret,
                RedirectUrl = model.RedirectUrl
            };

            return Ok(googleApp);
        }
    }
}
