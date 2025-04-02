using AdminPanel.Models.Auth;
using Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Implementations.Google;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserService _userService;
        private readonly DefaultValues _defaultValues;
        private readonly MetaService _metaService;
        private readonly GoogleService _googleService;

        public AuthController(JwtService jwtService, MetaService metaService, GoogleService googleService)
        {
            _jwtService = jwtService;
            _userService = new UserService();
            _defaultValues = new DefaultValues();
            _metaService = metaService;
            _googleService = googleService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            var passwordHash = _defaultValues.HashPassword(model.Password);
            var user = _userService.GetUserLogin(model.Email, passwordHash);

            if (user == null) return Ok(new { IsSuccess = false, Message = "Kullanıcı adı veya şifre yanlış!" });

            var roles = _userService.GetUserRole(user.Id).Select(ur => ur.Role.Name).ToList();
            var token = _jwtService.GenerateToken(user, roles);

            return Ok(new { IsSuccess = true, Token = token });
        }

		[Authorize]
        [HttpGet("control")]
        public IActionResult Control()
        {
            var userId = UserId();
            if (userId != 0)
            {
                var user = _userService.GetUserById(userId);
                if (user != null)
                {
                    userId = user.Id;
                }
                else
                {
                    userId = 0;
                }
            }
            return Ok(userId);
        }

        [Authorize]
        [HttpGet("meta-token-control")]
        public IActionResult MetaControl()
        {
            var userId = UserId();
            var meta = _metaService.GetLongAccessToken(userId);
            if (meta == null)
            {
                return Ok(0);
            }
            else
            {
                return Ok(1);
            }
        }

        [Authorize]
        [HttpGet("google-token-control")]
        public IActionResult GoogleControl()
        {
            var userId = UserId();
            var google = _googleService.GetGoogleAccessTokenControl(userId);
            if (google == null)
            {
                return Ok(0);
            }
            else
            {
                return Ok(1);
            }
        }

        [Authorize]
        [HttpGet("organization-control")]
        public IActionResult OrganizationControl()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            if (organization.GoogleAnalytics == null || organization.GoogleSearchConsole == null || organization.MetaAccount == null)
            {
                return Ok(0);
            }
            else
            {
                return Ok(1);
            }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
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
