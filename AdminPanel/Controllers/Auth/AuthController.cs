using AdminPanel.Models.Auth;
using Core.Data;
using Iyzipay.Model;
using Iyzipay.Request;
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
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly UserService _userService;
        private readonly DefaultValues _defaultValues;
        private readonly MetaService _metaService;
        private readonly GoogleService _googleService;

        public AuthController(JwtService jwtService, MetaService metaService, GoogleService googleService, IConfiguration configuration)
        {
            _jwtService = jwtService;
            _configuration = configuration;
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
            var lastActivity = _userService.UpdateLastActivity(user.Id);
            var token = _jwtService.GenerateToken(user, roles);

            return Ok(new { IsSuccess = true, Token = token });
        }

        [HttpPost("iyzico-callback")]
        public async Task<IActionResult> IyzipayCallback([FromForm] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token bulunamadı.");
            }

            var request = new RetrieveCheckoutFormRequest
            {
                Token = token
            };

            var _iyzicoOptions = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzico:ApiKey"],
                SecretKey = _configuration["Iyzico:SecretKey"],
                BaseUrl = _configuration["Iyzico:BaseUrl"],
            };

            var checkoutForm = CheckoutForm.Retrieve(request, _iyzicoOptions);
            var paymentStatus = checkoutForm.Result.PaymentStatus;
            if (paymentStatus == "SUCCESS")
            {
				var tokenControl = _userService.GetPlanToken(token);
				var payment = _userService.UpdatePaymentPlan(tokenControl.OrganizationId, true);
				return Redirect(_configuration["Iyzico:SuccessUrl"]);
            }
            return Redirect(_configuration["Iyzico:FailUrl"]);
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
        [HttpGet("ads-token-control")]
        public IActionResult AdsControl()
        {
            var userId = UserId();
            var google = _googleService.GetGoogleAdsAccessTokenControl(userId);
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
		[HttpGet("meta-control-lazy")]
		public IActionResult OrganizationMetaLazy()
		{
			var userId = UserId();
			var meta = _metaService.GetLongAccessToken(userId);
			if (meta == null)
			{
				return Ok(0);
			}
			else
			{
				var user = _userService.GetUserById(userId);
				var organization = _userService.GetOrganizationById(user.OrganizationId);
				if (organization.MetaAccount == null)
				{
					return Ok(0);
				}
				else
				{
					return Ok(1);
				}
			}
		}

		[Authorize]
		[HttpGet("google-control-lazy")]
		public IActionResult OrganizationGoogleLazy()
		{
			var userId = UserId();
			var google = _googleService.GetGoogleAccessTokenControl(userId);
			if (google == null)
			{
				return Ok(0);
			}
			else
			{
				var user = _userService.GetUserById(userId);
				var organization = _userService.GetOrganizationById(user.OrganizationId);
				if (organization.GoogleAnalytics == null || organization.GoogleSearchConsole == null)
				{
					return Ok(0);
				}
				else
				{
					return Ok(1);
				}
			}
		}

		[Authorize]
		[HttpGet("ads-control-lazy")]
		public IActionResult OrganizationAdsLazy()
		{
			var userId = UserId();
			var google = _googleService.GetGoogleAdsAccessTokenControl(userId);
			if (google == null)
			{
				return Ok(0);
			}
			else
			{
				var user = _userService.GetUserById(userId);
				var organization = _userService.GetOrganizationById(user.OrganizationId);
				if (organization.GoogleAccount == null)
				{
					return Ok(0);
				}
				else
				{
					return Ok(1);
				}
			}
		}

		[Authorize]
		[HttpGet("meta-control")]
		public IActionResult OrganizationMeta()
		{
			var userId = UserId();
			var meta = _metaService.GetLongAccessToken(userId);
			if (meta == null)
			{
				return Ok(0);
			}
			else
			{
                var user = _userService.GetUserById(userId);
				var organization = _userService.GetOrganizationById(user.OrganizationId);
				if (organization.MetaAccount == null)
				{
					return Ok(1);
				}
				else
				{
					return Ok(2);
				}
			}
		}

		[Authorize]
		[HttpGet("google-control")]
		public IActionResult OrganizationGoogle()
		{
			var userId = UserId();
			var google = _googleService.GetGoogleAccessTokenControl(userId);
			if (google == null)
			{
				return Ok(0);
			}
			else
			{
				var user = _userService.GetUserById(userId);
				var organization = _userService.GetOrganizationById(user.OrganizationId);
				if (organization.GoogleAnalytics == null || organization.GoogleSearchConsole == null)
				{
					return Ok(1);
				}
				else
				{
					return Ok(2);
				}
			}
		}

		[Authorize]
		[HttpGet("ads-control")]
		public IActionResult OrganizationAds()
		{
			var userId = UserId();
			var google = _googleService.GetGoogleAdsAccessTokenControl(userId);
			if (google == null)
			{
				return Ok(0);
			}
			else
			{
				var user = _userService.GetUserById(userId);
				var organization = _userService.GetOrganizationById(user.OrganizationId);
				if (organization.GoogleAccount == null)
				{
					return Ok(1);
				}
				else
				{
					return Ok(2);
				}
			}
		}

		[Authorize]
		[HttpGet("controls")]
		public IActionResult OrganizationControls()
		{
			var userId = UserId();
			var googleAds = _googleService.GetGoogleAdsAccessTokenControl(userId);
			var meta = _metaService.GetLongAccessToken(userId);
			var google = _googleService.GetGoogleAccessTokenControl(userId);

            if (meta == null && googleAds == null && google == null)
            {
				return Ok(0);
            }
            else
            {
				return Ok(1);
            }
        }

		[Authorize]
		[HttpGet("menu-list")]
		public IActionResult MenuList()
		{
			var userId = UserId();
			var googleIsValid = true;
			var adsIsValid = true;
			var metaIsValid = true;
			var accountDetail = true;

			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
			var googleAds = _googleService.GetGoogleAdsAccessTokenControl(userId);
			var meta = _metaService.GetLongAccessToken(userId);
			var google = _googleService.GetGoogleAccessTokenControl(userId);

			if (google == null || organization.GoogleAnalytics == null || organization.GoogleSearchConsole == null)
			{
				googleIsValid = false;
			}

			if (meta == null || organization.MetaAccount == null)
			{
				metaIsValid = false;
			}

			if (googleAds == null || organization.GoogleAccount == null)
			{
				adsIsValid = false;
			}

            if (googleAds == null && meta == null && google == null)
            {
				accountDetail = false;
			}

            var result = new
			{
				google = googleIsValid,
				google_ads = adsIsValid,
				meta = metaIsValid,
				account_detail = accountDetail,
			};

			return Ok(result);
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
