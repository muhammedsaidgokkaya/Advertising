using AdminPanel.Controllers.Google.SearchConsole;
using AdminPanel.Models.Meta.AdvertisingAccount;
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
        public ActionResult<GetUserAndRole> GetUser(int userId = 0)
        {
            if (userId == 0)
            {
                userId = UserId();
            }
            
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            var role = _userService.GetUserRole(userId);

            var data = new GetUserAndRole
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
                Address = user.Address,
                Roles = role.Select(q => q.RoleId).ToList()
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
        public ActionResult<GetUserAndRole> GetAddUser()
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
                FirstName = "",
                LastName = "",
                Mail = "",
                Phone = "",
                Title = "",
                DateOfBirth = DateTime.UtcNow,
                Gender = "E",
                Address = ""
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

            _emailHelper.SendEmail(user.Mail, user.FirstName, username, password);

            if (user.Roles.Count != 0 && user.Roles != null)
            {
                foreach (var item in user.Roles)
                {
                    _userService.AddUserRole(newUser, item);
                }
            }

            return Ok(new
            {
                id = newUser
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-photo")]
        public IActionResult AddPhoto([FromForm] AddPhoto photo)
        {
            if (photo?.Photo != null)
            {
                var uploadsDirectory = @"C:\Users\furka\Desktop\project-template\public\user";

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                var fileExtension = Path.GetExtension(photo.Photo.FileName).ToLower();

                if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg")
                {
                    return BadRequest("Yalnızca .png, .jpg, .jpeg dosya uzantıları kabul edilmektedir.");
                }

                var fileName = photo.UserId + fileExtension;

                var filePath = Path.Combine(uploadsDirectory, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    photo.Photo.CopyTo(fileStream);
                }
            }

            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-admin-user")]
        public IActionResult UpdateAdminUser([FromBody] UpdateUser user)
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            DateTime? dateOfBirth = null;
            if (DateTime.TryParse(user.DateOfBirth, out var parsedDate))
            {
                dateOfBirth = parsedDate.ToUniversalTime();
            }
            var updateUser = _userService.UpdateAdminUser(userId, user.FirstName, user.LastName, user.Mail, user.Phone, user.Title, dateOfBirth, user.Gender, user.Address);
            var updateOrganization = _userService.UpdateOrganization(org.OrganizationId, user.Name, user.OrgAddress, user.ZipCode, user.TaskNumber);
            if (updateUser == 0 && updateOrganization == 0)
            {
                return BadRequest(new { success = false, message = "User could not be added." });
            }
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-user")]
        public IActionResult UpdateUser([FromBody] UpdateUser user)
        {
            DateTime? dateOfBirth = null;
            if (DateTime.TryParse(user.DateOfBirth, out var parsedDate))
            {
                dateOfBirth = parsedDate.ToUniversalTime();
            }
            var updateUser = _userService.UpdateAdminUser(user.Id, user.FirstName, user.LastName, user.Mail, user.Phone, user.Title, dateOfBirth, user.Gender, user.Address);
            if (updateUser == 0)
            {
                return BadRequest(new { success = false, message = "User could not be added." });
            }

            _userService.RemoveUserRolesByUserId(updateUser);

            if (user.Roles.Count != 0 && user.Roles != null)
            {
                foreach (var item in user.Roles)
                {
                    _userService.AddUserRole(updateUser, item);
                }
            }

            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-photo")]
        public IActionResult UpdatePhoto([FromForm] AddPhoto photo)
        {
            if (photo?.Photo != null)
            {
                var uploadsDirectory = @"C:\Users\furka\Desktop\project-template\public\user";

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                var fileExtension = Path.GetExtension(photo.Photo.FileName).ToLower();

                if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg")
                {
                    return BadRequest("Yalnızca .png, .jpg, .jpeg dosya uzantıları kabul edilmektedir.");
                }

                var fileNameWithoutExtension = photo.UserId.ToString();

                var existingFiles = Directory.GetFiles(uploadsDirectory, fileNameWithoutExtension + ".*");
                foreach (var existingFile in existingFiles)
                {
                    var existingFileExtension = Path.GetExtension(existingFile).ToLower();
                    if (existingFileExtension == ".png" || existingFileExtension == ".jpg" || existingFileExtension == ".jpeg")
                    {
                        System.IO.File.Delete(existingFile);
                    }
                }

                var newFileName = fileNameWithoutExtension + fileExtension;
                var newFilePath = Path.Combine(uploadsDirectory, newFileName);

                using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                {
                    photo.Photo.CopyTo(fileStream);
                }

                return Ok(new { success = true });
            }

            return BadRequest("Geçerli bir fotoğraf yüklenmedi.");
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("update-user-password")]
        public IActionResult UpdatePassword([FromBody] UpdatePassword user)
        {
            var userId = UserId();
            var newUser = _userService.UpdatePassword(userId, user.NewPassword);
            if (newUser == 0)
            {
                return Ok(new { success = false });
            }
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete-user")]
        public IActionResult DeleteUser(int userId)
        {
            var user = _userService.IsDeletedUser(userId);
            if (user == 0)
            {
                return Ok(new { success = false });
            }
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete-users")]
        public IActionResult DeleteUsers([FromBody] DeleteUsers user)
        {
            foreach (var item in user.UserId)
            {
                var deleteUser = _userService.IsDeletedUser(item);
                if (deleteUser == 0)
                {
                    return Ok(new { success = false });
                }
            }
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("status-user")]
        public IActionResult StatusUser(int userId)
        {
            var user = _userService.IsActiveUser(userId);
            if (user == 0)
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
