using AdminPanel.Models.Organization.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI.API.Completions;
using OpenAI.API;
using Service.Implementations.User;
using Utilities.Helper;
using Google.Protobuf.Collections;
using AdminPanel.Models.Auth;

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
            var hashCode = _defaultValues.GenerateUniqueCode(account.Name);

			var organization = _userService.AddOrganization(account.Name, hashCode, 5, 2, account.AccountType, account.OrgAddress, account.ZipCode, account.TaskNumber, account.Phone);
            
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
            _userService.AddUserRole(newUser, 2);
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
                var uploadsDirectory = @"C:\Users\furka\Desktop\dijitals\public\user";

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                var fileExtension = Path.GetExtension(photo.Photo.FileName).ToLower();

                if (fileExtension != ".png")
                {
                    return BadRequest("Yalnızca .png dosya uzantıları kabul edilmektedir.");
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

		[HttpPost("check-mail")]
		public async Task<IActionResult> CheckMail([FromBody] CheckMail request)
		{
            var userCheck = _userService.GetUserCheckMail(request.Mail).ToList();
            if (userCheck.Count != 0)
            {
				return Ok(0);
			}

			return Ok(1);
		}
	}
}
