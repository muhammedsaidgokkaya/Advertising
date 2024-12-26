using AdminPanel.Controllers.Google.SearchConsole;
using AdminPanel.Models.Organization.User;
using Core.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Implementations.Google;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;
using Utilities.Utilities.MetaData;
using static AdminPanel.Controllers.Auth.AuthController;

namespace AdminPanel.Controllers.Organization
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger<OrganizationController> _logger;
        private readonly UserService _userService;
        private readonly DefaultValues _defaultValues;

        public OrganizationController(ILogger<OrganizationController> logger)
        {
            _logger = logger;
            _userService = new UserService();
            _defaultValues = new DefaultValues();
        }

        [HttpGet("user")]
        public ActionResult<GetUser> GetUser()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);

            var data = new GetUser
            {
                Name = organization.Name,
                TaskNumber = organization.TaskNumber,
                OrgAddress = organization.Address,
                ZipCode = organization.ZipCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mail = user.Mail,
                Phone = user.Phone,
                Title = user.Title,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address
            };

            return Ok(data);
        }

        [HttpGet("drawer")]
        public ActionResult<Drawer> GetDrawer()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);

            var data = new Drawer
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mail = user.Mail
            };

            return Ok(data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-user")]
        public IActionResult UpdateUser([FromBody] UpdateUser user)
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            DateTime? dateOfBirth = null;
            if (DateTime.TryParse(user.DateOfBirth, out var parsedDate))
            {
                dateOfBirth = parsedDate.ToUniversalTime();
            }
            var newUser = _userService.UpdateAdminUser(userId, user.FirstName, user.LastName, user.Mail, user.Phone, user.Title, dateOfBirth, user.Gender, user.Address);
            var newOrganization = _userService.UpdateOrganization(org.OrganizationId, user.Name, user.OrgAddress, user.ZipCode, user.TaskNumber);
            if (newUser == 0 && newOrganization == 0)
            {
                return Ok(new { success = false });
            }
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-user-password")]
        public IActionResult UpdatePassword([FromBody] Password user)
        {
            var userId = UserId();
            var newUser = _userService.UpdatePassword(userId, user.NewPassword);
            if (newUser == 0)
            {
                return Ok(new { success = false });
            }
            return Ok(new { success = true });
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
