using AdminPanel.Models.Auth;
using Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Implementations.User;

namespace AdminPanel.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserService _userService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
            _userService = new UserService();
        }

        [HttpPost("login")]
        public IActionResult Login(string userName, string password)
        {
            var user = _userService.GetUserLogin(userName, password);

            if (user == null) return Unauthorized("Invalid credentials");

            var roles = _userService.GetUserRole(user.Id).Select(ur => ur.Role.Name).ToList();
            var token = _jwtService.GenerateToken(user, roles);

            return Ok(new { Token = token });
        }
    }
}
