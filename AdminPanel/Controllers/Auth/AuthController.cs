using AdminPanel.Models.Auth;
using Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Implementations.User;
using Utilities.Helper;

namespace AdminPanel.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserService _userService;
        private readonly DefaultValues _defaultValues;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
            _userService = new UserService();
            _defaultValues = new DefaultValues();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            var passwordHash = _defaultValues.HashPassword(model.Password);
            var user = _userService.GetUserLogin(model.UserName, passwordHash);

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
            }
            return Ok(userId);
        }

        public class LoginRequest
        {
            public string UserName { get; set; }
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
