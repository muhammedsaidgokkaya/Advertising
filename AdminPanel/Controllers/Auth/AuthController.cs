using AdminPanel.Models.Auth;
using Core.Data;
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

            if (user == null) return Unauthorized("Invalid credentials");

            var roles = _userService.GetUserRole(user.Id).Select(ur => ur.Role.Name).ToList();
            var token = _jwtService.GenerateToken(user, roles);

            return Ok(new { Token = token });
        }

        public class LoginRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
