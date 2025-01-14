using AdminPanel.Models.Organization.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.User;
using Utilities.Helper;

namespace AdminPanel.Controllers.Organization
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly UserService _userService;
        private readonly DefaultValues _defaultValues;
        private readonly EmailHelper _emailHelper;

        public RegisterController(ILogger<RegisterController> logger)
        {
            _logger = logger;
            _userService = new UserService();
            _defaultValues = new DefaultValues();
            _emailHelper = new EmailHelper();
        }

        [HttpPost("add-account")]
        public IActionResult AddAccount([FromBody] AddAccount account)
        {
            var organization = _userService.AddOrganization(account.Name, 5, 2, account.OrgAddress, account.ZipCode, account.TaskNumber, account.Phone);
            
            DateTime? dateOfBirth = null;
            if (DateTime.TryParse(account.DateOfBirth, out var parsedDate))
            {
                dateOfBirth = parsedDate.ToUniversalTime();
            }

            var password = _defaultValues.GenerateRandomPassword();

            var passwordHash = _defaultValues.HashPassword(password);

            var newUser = _userService.AddUser(organization, account.FirstName, account.LastName, account.Mail,
                account.Phone, account.Title, dateOfBirth, account.Gender, account.Address, "", passwordHash);

            if (newUser == 0)
            {
                return BadRequest(new { success = false, message = "User could not be added." });
            }

            var firstName = _defaultValues.RemoveDiacritics(account.FirstName.ToLower());
            var lastName = _defaultValues.RemoveDiacritics(account.LastName.ToLower());
            var username = firstName + "." + lastName;

            _userService.UpdateUserName(newUser, username);
            _emailHelper.SendEmail(account.Mail, account.FirstName, username, password);

            _userService.AddUserRole(newUser, 1);

            return Ok(new
            {
                id = newUser
            });
        }

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
    }
}
