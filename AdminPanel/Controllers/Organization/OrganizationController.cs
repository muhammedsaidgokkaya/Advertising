using AdminPanel.Controllers.Google.SearchConsole;
using AdminPanel.Models.Organization.Role;
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
        private readonly EmailHelper _emailHelper;

        public OrganizationController(ILogger<OrganizationController> logger)
        {
            _logger = logger;
            _userService = new UserService();
            _defaultValues = new DefaultValues();
            _emailHelper = new EmailHelper();
        }

        [HttpGet("users")]
        public ActionResult<IEnumerable<Users>> GetUsers()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var users = _userService.GetUsers(user.OrganizationId, userId);

            var userList = users.Select(user => new Users
            {
                Id = user.Id,
                Name = user.FirstName + " " + user.LastName,
                Mail = user.Mail,
                Phone = user.Phone,
                Title = user.Title,
                DateOfBirth = user.DateOfBirth.HasValue 
                ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") 
                : "Belirtilmemiş",
                Gender = user.Gender == "E" ? "Erkek" : user.Gender == "K" ? "Kız" : "Belirtilmemiş",
                IsActive = user.IsActive ? "Aktif" : "Pasif",
            }).ToList();

            return Ok(userList);
        }

        [HttpGet("user")]
        public ActionResult<GetUser> GetUser(int userId = 0)
        {
            if (userId == 0)
            {
                userId = UserId();
            }
            
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

        [HttpGet("get-add-user")]
        public ActionResult<GetUser> GetAddUser()
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
                FirstName = "Ad",
                LastName = "Soyad",
                Mail = "mail@example.com",
                Phone = "+905000000000",
                Title = "Ünvan",
                DateOfBirth = DateTime.UtcNow,
                Gender = "E",
                Address = "Adres"
            };

            return Ok(data);
        }

        [HttpGet("roles")]
        public ActionResult<IEnumerable<Roles>> GetRoles()
        {
            var roles = _userService.GetRole();

            var roleList = roles.Select(role => new Roles
            {
                Id = role.Id,
                Name = role.Name
            }).ToList();

            return Ok(roleList);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-user")]
        public IActionResult AddUser([FromBody] AddUser user)
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);

            DateTime? dateOfBirth = null;
            if (DateTime.TryParse(user.DateOfBirth, out var parsedDate))
            {
                dateOfBirth = parsedDate.ToUniversalTime();
            }

            var firstName = _defaultValues.RemoveDiacritics(user.FirstName.ToLower());
            var lastName = _defaultValues.RemoveDiacritics(user.LastName.ToLower());
            var username = firstName + "." + lastName;

            var password = _defaultValues.GenerateRandomPassword();

            var newUser = _userService.AddUser(org.OrganizationId, user.FirstName, user.LastName, user.Mail,
                user.Phone, user.Title, dateOfBirth, user.Gender, user.Address, "photo", username, password);

            if (newUser == 0)
            {
                return BadRequest(new { success = false, message = "User could not be added." });
            }

            _emailHelper.SendEmail(user.Mail, username, password);

            foreach (var item in user.Roles)
            {
                _userService.AddUserRole(newUser, item);
            }
            
            return Ok(new { success = true });
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
