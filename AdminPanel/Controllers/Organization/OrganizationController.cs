using AdminPanel.Controllers.Google.SearchConsole;
using AdminPanel.Models.Auth;
using AdminPanel.Models.Meta.AdvertisingAccount;
using AdminPanel.Models.Organization.Plan;
using AdminPanel.Models.Organization.Role;
using AdminPanel.Models.Organization.User;
using Core.Domain.User;
using Iyzipay.Model.V2.Subscription;
using Iyzipay.Model;
using Iyzipay.Request.V2.Subscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Implementations.Calendar;
using Service.Implementations.Google;
using Service.Implementations.Meta;
using Service.Implementations.Task;
using Service.Implementations.User;
using SixLabors.ImageSharp;
using System.ComponentModel.Design;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;
using Utilities.Utilities.MetaData;
using static AdminPanel.Controllers.Auth.AuthController;
using AdminPanel.Helpers;
using Iyzipay.Request;
using static Google.Rpc.Context.AttributeContext.Types;
using System.Numerics;
using Org.BouncyCastle.Asn1.Ocsp;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Xml;

namespace AdminPanel.Controllers.Organization
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger<OrganizationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly DefaultValues _defaultValues;
        private readonly EmailHelper _emailHelper;
		private readonly TaskService _taskService;
		private readonly CalendarService _calendarService;

		public OrganizationController(ILogger<OrganizationController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = new UserService();
            _defaultValues = new DefaultValues();
            _emailHelper = new EmailHelper();
			_taskService = new TaskService();
			_calendarService = new CalendarService();
		}

		[HttpPost("check-mail-me")]
		public async Task<IActionResult> CheckMailMe([FromBody] CheckMailRequest request)
		{
			var userCheck = _userService.GetUserCheckMail(request.Mail, request.UserId).ToList();
			if (userCheck.Count != 0)
			{
				return Ok(0);
			}

			return Ok(1);
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

        [HttpGet("admin-user")]
        public ActionResult<GetAdminUserAndRole> GetAdminUser(int userId = 0)
        {
            if (userId == 0)
            {
                userId = UserId();
            }

            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            var role = _userService.GetUserRole(userId);

            var data = new GetAdminUserAndRole
            {
                Name = organization.Name,
                TaskNumber = organization.TaskNumber,
                OrgAddress = organization.Address,
                ZipCode = organization.ZipCode,
                AccountType = organization.AccountType,
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

        [HttpGet("admin-payment-user")]
        public ActionResult<PaymentOrg> GetAdminPaymentUser(int userId = 0)
        {
            if (userId == 0)
            {
                userId = UserId();
            }

            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            var role = _userService.GetUserRole(userId);

            var data = new PaymentOrg
            {
                Name = organization.Name,
                TaskNumber = organization.TaskNumber,
                OrgAddress = organization.Address,
                ZipCode = organization.ZipCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mail = user.Mail,
                Phone = organization.Phone,
            };

            return Ok(data);
        }

        [HttpGet("account-count")]
        public ActionResult<AccountCount> GetOrganizationAccountCount()
        {
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
            var metaCount = organization.AccountCount;
            var adsCount = organization.AccountCount;
            var searchCount = organization.AccountCount;
            var analyticsCount = organization.AccountCount;

			var metaAccount = organization.MetaAccount;
            if (metaAccount != null)
            {
				var accounts = metaAccount?.Split(',')
				.Select(account =>
				{
					var parts = account.Split('/');
					return new
					{
						AccountId = parts[0].Trim(),
						Account = parts.Length > 1 ? parts[1].Trim() : string.Empty
					};
				})
				.ToList();

				metaCount = organization.AccountCount - accounts.Count;
			}

            var adsAccount = organization.GoogleAccount;
            if (adsAccount != null)
            {
				var result = adsAccount
			       .Split(',')
			       .Select(accountInfo =>
			       {
				       var parts = accountInfo.Split('/');
				       return new
				       {
					       account = parts[0],
					       accountId = parts[1]
				       };
			       })
			       .ToList();

                adsCount = organization.AccountCount - result.Count;
			}

            var searchAccount = organization.GoogleSearchConsole;
            if (searchAccount != null) 
            {
				var result = searchAccount
				.Split(',')
				.Select(url => new { account = url.Trim(), accountId = url.Trim() })
				.ToList();

                searchCount = organization.AccountCount - result.Count;
			}

			var analyticsAccount = organization.GoogleAnalytics;
            if (analyticsAccount != null)
            {
				var result = analyticsAccount
				    .Split(',')
				    .Select(accountInfo =>
				    {
					    var parts = accountInfo.Split('/');
					    return new
					    {
						    account = parts[0],
						    accountId = parts[2]
					    };
				    })
				    .ToList();

                analyticsCount = organization.AccountCount - result.Count;
			}
			
			var data = new AccountCount
			{
				MetaCount = metaCount,
                AdsCount = adsCount,
                SearchCount = searchCount,
                AnalyticsCount = analyticsCount,
                TotalCount = organization.AccountCount,
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

		[HttpGet("workspace")]
		public ActionResult<GetOrganization> GetWorkspace()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);

			var data = new GetOrganization
			{
				Id = organization.Id,
				Name = organization.Name,
				Plan = organization.AccountType == "individual" ? "Bireysel" : "Kurumsal"
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

		[HttpGet("departments")]
		public ActionResult<IEnumerable<Users>> GetOrganizationDepartments()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var users = _userService.GetUserTitles(user.OrganizationId, userId);
			return Ok(users);
		}

		[HttpGet("dashboard-task-calendar")]
		public ActionResult<TaskAndCalendar> GetOrganizationDashboard()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var tasks = _taskService.GetTasksCount(user.OrganizationId);
			var calendar = _calendarService.GetCalendersCount(user.OrganizationId);
			var request = _calendarService.GetCalendersRequestCount(user.OrganizationId);

			var data = new TaskAndCalendar
			{
				TaskCount = tasks,
				RequestCount = request,
				CalendarCount = calendar,
			};

			return Ok(data);
		}

		[HttpGet("department-users")]
		public ActionResult<IEnumerable<Users>> GetDepartmentUsers([FromQuery] List<string> department)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
            var departmentUsers = _userService.GetDepartmentUsers(user.OrganizationId, userId, department);

			var userList = departmentUsers.Select(user => new Users
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

		[HttpGet("hash-code")]
		public ActionResult<OrganizationHashCode> GetHashCode()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);

			var data = new OrganizationHashCode
			{
				HashCode = organization.OrganizationHashCode,
			};

			return Ok(data);
		}

        [HttpGet("card")]
        public async Task<IActionResult> ListFirstCard()
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            var card = _userService.GetCard(org.OrganizationId);

            var _iyzicoOptions = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzico:ApiKey"],
                SecretKey = _configuration["Iyzico:SecretKey"],
                BaseUrl = _configuration["Iyzico:BaseUrl"],
            };

            if (card != null)
            {
                var request = new RetrieveCardListRequest
                {
                    CardUserKey = card.CardUserKey,
                    Locale = Locale.TR.ToString(),
                    ConversationId = org.OrganizationId.ToString(),
                };

                var result = await CardList.Retrieve(request, _iyzicoOptions);

                if (result.Status != "success" || result.CardDetails == null || !result.CardDetails.Any())
                {
                    return NotFound("Kullanıcının kayıtlı kartı bulunamadı.");
                }

                var firstCard = result.CardDetails.First();

                return Ok(firstCard);
            }
            else
            {
                return BadRequest("");
            }
        }

        [HttpGet("plan")]
        public ActionResult<AdminPanel.Models.Organization.Plan.Plan> GetPlan()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetPlan(user.OrganizationId);
            if (organization == null)
                return NotFound("Plan bilgisi bulunamadı.");
            var data = new AdminPanel.Models.Organization.Plan.Plan
            {
                Amount = organization.Amount,
                PlanId = organization.PlanId,
                IsYearly = organization.IsYearly,
                IsPayment = organization.IsPayment,
            };

            return Ok(data);
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

            var passwordHash = _defaultValues.HashPassword(password);

            var newUser = _userService.AddUser(org.OrganizationId, user.FirstName, user.LastName, user.Mail,
                user.Phone, user.Title, dateOfBirth, user.Gender, user.Address, username, passwordHash);

            if (newUser == 0)
            {
                return BadRequest(new { success = false, message = "User could not be added." });
            }

            _emailHelper.SendEmail(user.Mail, user.FirstName, username, password);

            var filteredRoles = user.Roles?.Where(role => role != 2 && role != 9).ToList();

            if (filteredRoles != null && filteredRoles.Count != 0)
            {
                foreach (var item in filteredRoles)
                {
                    _userService.AddUserRole(newUser, item);
                }
            }

            _userService.AddUserRole(newUser, 2);
            _userService.AddUserRole(newUser, 9);

            return Ok(new
            {
                id = newUser
            });
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

        [HttpPost("update-only-user")]
        public IActionResult UpdateOnlyUser([FromBody] UpdateOnlyUser user)
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            DateTime? dateOfBirth = null;
            if (DateTime.TryParse(user.DateOfBirth, out var parsedDate))
            {
                dateOfBirth = parsedDate.ToUniversalTime();
            }

            var updateUser = _userService.UpdateOnlyUser(userId, user.FirstName, user.LastName, user.Mail, user.Phone, dateOfBirth, user.Gender);
            
            if (updateUser == 0)
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

            var filteredRoles = user.Roles?.Where(role => role != 2 && role != 9).ToList();

            if (filteredRoles != null && filteredRoles.Count != 0)
            {
                foreach (var item in filteredRoles)
                {
                    _userService.AddUserRole(updateUser, item);
                }
            }

            _userService.AddUserRole(updateUser, 2);
            _userService.AddUserRole(updateUser, 9);

            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-photo")]
        public IActionResult UpdatePhoto([FromForm] AddPhoto photo)
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

                var fileNameWithoutExtension = photo.UserId.ToString();

                var existingFiles = Directory.GetFiles(uploadsDirectory, fileNameWithoutExtension + ".*");
                foreach (var existingFile in existingFiles)
                {
                    var existingFileExtension = Path.GetExtension(existingFile).ToLower();
                    if (existingFileExtension == ".png")
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

        [HttpPost("save-meta-selections")]
        public IActionResult SaveMetaSelections([FromBody] SelectedAdvertisingAccounts payload)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
			if (payload.SelectedAdvertisingAccount.Count == 0)
			{
				return Ok(new { success = false, message = "Meta ads hesabı seçtiğinize emin olun!" });
			}
			var metaAccounts = payload.SelectedAdvertisingAccount.ToList();
            var metaAccount = string.Join(",", metaAccounts.Select(account => $"{account.Id}/{account.Name}"));

            if (organization.MetaAccount != null)
            {
                var metas = organization.MetaAccount + "," + metaAccount;
				var updateOrganization = _userService.UpdateMetaAccountOrganization(organization.Id, metas);

				if (updateOrganization == 0)
				{
					return Ok(new { success = false, message = "Meta ads hesabı seçtiğinize emin olun!" });
				}

				return Ok(new { success = true });
			}

            if (metaAccount != "")
            {
                var updateOrganization = _userService.UpdateMetaAccountOrganization(organization.Id, metaAccount);

                if (updateOrganization == 0)
                {
                    return Ok(new { success = false, message = "Meta ads hesabı seçtiğinize emin olun!" });
                }

                return Ok(new { success = true });
            }
            return Ok(new { success = false, message = "Meta ads hesabı seçtiğinize emin olun!" });
        }

        [HttpPost("save-analytics-selections")]
        public IActionResult SaveAnalyticsSelections([FromBody] SelectedAnalyticss payload)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
			if (payload.SelectedAnalytics.Count == 0)
			{
				return Ok(new { success = false, message = "Analytics hesabı seçtiğinize emin olun!" });
			}
			var analyticsList = payload.SelectedAnalytics.ToList();
            var googleAnalytics = string.Join(",",
                analyticsList.Select(account =>
                    $"{account.DisplayName}/{string.Join("/", account.PropertySummaries.Select(property => property.Property))}"
                )
            );

			if (organization.GoogleAnalytics != null)
			{
				var metas = organization.GoogleAnalytics + "," + googleAnalytics;
				var updateOrganization = _userService.UpdateAnalyticsAccountOrganization(organization.Id, metas);

				if (updateOrganization == 0)
				{
					return Ok(new { success = false, message = "Analytics hesabı seçtiğinize emin olun!" });
				}

				return Ok(new { success = true });
			}

			if (googleAnalytics != "")
            {
                var updateOrganization = _userService.UpdateAnalyticsAccountOrganization(organization.Id, googleAnalytics);

                if (updateOrganization == 0)
                {
                    return Ok(new { success = false, message = "Analytics hesabı seçtiğinize emin olun!" });
                }

                return Ok(new { success = true });
            }
            return Ok(new { success = false, message = "Analytics hesabı seçtiğinize emin olun!" });
        }

        [HttpPost("save-search-console-selections")]
        public IActionResult SaveSearchConsoleSelections([FromBody] SelectedSitess payload)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            if (payload.SelectedSites.Count == 0)
            {
				return Ok(new { success = false, message = "Search Console hesabı seçtiğinize emin olun!" });
			}
            var searchUrl = payload.SelectedSites.ToList();
            var googleSearchConsole = string.Join(",", searchUrl.Select(site => site.SiteUrl));

			if (organization.GoogleSearchConsole != null)
			{
				var metas = organization.GoogleSearchConsole + "," + googleSearchConsole;
				var updateOrganization = _userService.UpdateSearchConsoleAccountOrganization(organization.Id, metas);

				if (updateOrganization == 0)
				{
					return Ok(new { success = false, message = "Search Console hesabı seçtiğinize emin olun!" });
				}

				return Ok(new { success = true });
			}

			if (googleSearchConsole != "")
            {
                var updateOrganization = _userService.UpdateSearchConsoleAccountOrganization(organization.Id, googleSearchConsole);

                if (updateOrganization == 0)
                {
                    return Ok(new { success = false, message = "Search Console hesabı seçtiğinize emin olun!" });
                }

                return Ok(new { success = true });
            }
            return Ok(new { success = false, message = "Search Console hesabı seçtiğinize emin olun!" });
        }

        [HttpPost("save-ads-selections")]
        public IActionResult SaveAdsSelections([FromBody] SelectedAdsAccounts payload)
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
			if (payload.SelectedAdsAccount.Count == 0)
			{
				return Ok(new { success = false, message = "Google Ads hesabı seçtiğinize emin olun!" });
			}
			var adsAccounts = payload.SelectedAdsAccount.ToList();
            var adsAccount = string.Join(",", adsAccounts.Select(account => $"{account.Name}/{account.Id}"));

			if (organization.GoogleAccount != null)
			{
				var metas = organization.GoogleAccount + "," + adsAccount;
				var updateOrganization = _userService.UpdateAdsAccountOrganization(organization.Id, metas);

				if (updateOrganization == 0)
				{
					return Ok(new { success = false, message = "Google Ads hesabı seçtiğinize emin olun!" });
				}

				return Ok(new { success = true });
			}

			if (adsAccount != "")
            {
                var updateOrganization = _userService.UpdateAdsAccountOrganization(organization.Id, adsAccount);

                if (updateOrganization == 0)
                {
                    return Ok(new { success = false, message = "Google Ads hesabı seçtiğinize emin olun!" });
                }

                return Ok(new { success = true });
            }
            return Ok(new { success = false, message = "Google Ads hesabı seçtiğinize emin olun!" });
        }

        [HttpPost("update-user-password")]
        public IActionResult UpdatePassword([FromBody] UpdatePassword user)
        {
            var userId = UserId();
            var passwordHash = _defaultValues.HashPassword(user.NewPassword);
            var newUser = _userService.UpdatePassword(userId, passwordHash);
            if (newUser == 0)
            {
                return Ok(new { success = false });
            }
            return Ok(new { success = true });
        }

        [HttpPost("add-card")]
        public async Task<IActionResult> AddCard([FromBody] AddCard request)
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(org.OrganizationId);
            var card = _userService.GetCard(org.OrganizationId);

            string[] dateParts = request.ExpirationDate.Split('/');
            if (dateParts.Length != 2)
            {
                return BadRequest("Geçersiz tarih formatı. Beklenen format: MM/YYYY");
            }

            string expireMonth = dateParts[0].PadLeft(2, '0');
            string expireYear = dateParts[1];

            var _iyzicoOptions = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzico:ApiKey"],
                SecretKey = _configuration["Iyzico:SecretKey"],
                BaseUrl = _configuration["Iyzico:BaseUrl"],
            };

            if (card != null)
            {
                var deleteRequest = new DeleteCardRequest
                {
                    CardUserKey = card.CardUserKey,
                    CardToken = card.CardToken,
                    Locale = Locale.TR.ToString(),
                    ConversationId = org.OrganizationId.ToString()
                };

                var result = await Iyzipay.Model.Card.Delete(deleteRequest, _iyzicoOptions);
            }

            string lastFourDigits = request.CardNumber.Length >= 4
                ? request.CardNumber.Substring(request.CardNumber.Length - 4)
                : "****";

            string cardAlias = $"**** **** **** {lastFourDigits}";

            var cards = new CreateCardRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = org.OrganizationId.ToString(),
                Email = org.Mail,
                ExternalId = org.OrganizationId.ToString(),
                Card = new CardInformation
                {
                    CardAlias = cardAlias,
                    CardHolderName = request.CardHolder,
                    CardNumber = request.CardNumber,
                    ExpireMonth = expireMonth,
                    ExpireYear = expireYear
                }
            };

            var cardResponse = await Iyzipay.Model.Card.Create(cards, _iyzicoOptions);
            if (cardResponse.Status == "success")
            {
                var addOrUpdateCard = _userService.AddOrUpdateCard(org.OrganizationId, cardResponse.CardUserKey, cardResponse.CardToken, cardResponse.CardAlias);

                if (addOrUpdateCard == 0)
                {
                    return BadRequest(new { success = false, message = "User could not be added." });
                }
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

        #region Payment
        [HttpPost("live-payment")]
        public async Task<IActionResult> LivePayment([FromBody] AddSubscription request)
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            var priceUSD = _userService.GetSubscription(request.PlanId, request.IsYearly);
            var card = _userService.GetCard(org.OrganizationId);

            var _iyzicoOptions = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzico:ApiKey"],
                SecretKey = _configuration["Iyzico:SecretKey"],
                BaseUrl = _configuration["Iyzico:BaseUrl"],
            };

            var priceInTRY = CalculatePriceInTRY(Convert.ToDecimal(priceUSD.Price));

            var paymentRequest = new CreatePaymentRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Price = priceInTRY.ToString("F2", CultureInfo.InvariantCulture),
                PaidPrice = priceInTRY.ToString("F2", CultureInfo.InvariantCulture),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                Installment = 1,
                BasketId = "basket-001",
                PaymentChannel = Iyzipay.Model.PaymentChannel.WEB.ToString(),
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),

                PaymentCard = new Iyzipay.Model.PaymentCard
                {
                    CardToken = card.CardToken,
                    CardUserKey = card.CardUserKey
                },

                Buyer = new Iyzipay.Model.Buyer
                {
                    Id = userId.ToString(),
                    Name = request.Name,
                    Surname = "-",
                    GsmNumber = request.Phone,
                    Email = request.Mail,
                    IdentityNumber = request.Identity,
                    RegistrationAddress = request.OrgAddress,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    City = request.OrgAddress,
                    Country = "Turkey"
                },

                BillingAddress = new Iyzipay.Model.Address
                {
                    ContactName = request.Name,
                    City = request.OrgAddress,
                    Country = "Turkey",
                    Description = "Fatura Adresi",
                    ZipCode = request.Zip
                },

                ShippingAddress = new Iyzipay.Model.Address
                {
                    ContactName = request.Name,
                    City = request.OrgAddress,
                    Country = "Turkey",
                    Description = "Teslimat Adresi",
                    ZipCode = request.Zip
                },

                BasketItems = new List<Iyzipay.Model.BasketItem>
                {
                    new Iyzipay.Model.BasketItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Abonelik Paketi",
                        Category1 = "Abonelik",
                        ItemType = Iyzipay.Model.BasketItemType.VIRTUAL.ToString(),
                        Price = priceInTRY.ToString("F2", CultureInfo.InvariantCulture)
                    }
                }
            };

            var payment = await Iyzipay.Model.Payment.Create(paymentRequest, _iyzicoOptions);

            if (payment.Status == "success")
            {
                var addplan = _userService.AddOrUpdatePlan(org.OrganizationId, priceUSD.Price, request.PlanId, request.IsYearly, true);
                var nextAmount = _userService.UpdateNextPaymentDatePlan(addplan, request.IsYearly);

                return Ok(payment);
            }
            else
            {
                return Ok(payment);
            }
        }

        [HttpPost("live-new-card-payment")]
        public async Task<IActionResult> LiveNewCardPayment([FromBody] AddNewCardSubscription request)
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(org.OrganizationId);
            var priceUSD = _userService.GetSubscription(request.PlanId, request.IsYearly);
            var card = _userService.GetCard(org.OrganizationId);

            string[] dateParts = request.ExpirationDate.Split('/');
            if (dateParts.Length != 2)
            {
                return BadRequest("Geçersiz tarih formatı. Beklenen format: MM/YYYY");
            }

            string expireMonth = dateParts[0].PadLeft(2, '0');
            string expireYear = dateParts[1];

            var _iyzicoOptions = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzico:ApiKey"],
                SecretKey = _configuration["Iyzico:SecretKey"],
                BaseUrl = _configuration["Iyzico:BaseUrl"],
            };

            if (card != null)
            {
                var deleteRequest = new DeleteCardRequest
                {
                    CardUserKey = card.CardUserKey,
                    CardToken = card.CardToken,
                    Locale = Locale.TR.ToString(),
                    ConversationId = org.OrganizationId.ToString()
                };

                var result = await Iyzipay.Model.Card.Delete(deleteRequest, _iyzicoOptions);
            }

            string lastFourDigits = request.CardNumber.Length >= 4
                ? request.CardNumber.Substring(request.CardNumber.Length - 4)
                : "****";

            string cardAlias = $"**** **** **** {lastFourDigits}";

            var cards = new CreateCardRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = org.OrganizationId.ToString(),
                Email = org.Mail,
                ExternalId = org.OrganizationId.ToString(),
                Card = new CardInformation
                {
                    CardAlias = cardAlias,
                    CardHolderName = request.CardHolder,
                    CardNumber = request.CardNumber,
                    ExpireMonth = expireMonth,
                    ExpireYear = expireYear
                }
            };

            var cardResponse = await Iyzipay.Model.Card.Create(cards, _iyzicoOptions);
            if (cardResponse.Status == "success")
            {
                var addOrUpdateCard = _userService.AddOrUpdateCard(org.OrganizationId, cardResponse.CardUserKey, cardResponse.CardToken, cardResponse.CardAlias);

                if (addOrUpdateCard == 0)
                {
                    return BadRequest(new { success = false, message = "User could not be added." });
                }
            }
            else
            {
                return Ok(cardResponse);
            }

            var priceInTRY = CalculatePriceInTRY(Convert.ToDecimal(priceUSD.Price));

            var newCard = _userService.GetCard(org.OrganizationId);

            var paymentRequest = new CreatePaymentRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Price = priceInTRY.ToString("F2", CultureInfo.InvariantCulture),
                PaidPrice = priceInTRY.ToString("F2", CultureInfo.InvariantCulture),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                Installment = 1,
                BasketId = "basket-001",
                PaymentChannel = Iyzipay.Model.PaymentChannel.WEB.ToString(),
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),

                PaymentCard = new Iyzipay.Model.PaymentCard
                {
                    CardToken = newCard.CardToken,
                    CardUserKey = newCard.CardUserKey
                },

                Buyer = new Iyzipay.Model.Buyer
                {
                    Id = userId.ToString(),
                    Name = request.Name,
                    Surname = "-",
                    GsmNumber = request.Phone,
                    Email = request.Mail,
                    IdentityNumber = request.Identity,
                    RegistrationAddress = request.OrgAddress,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    City = request.OrgAddress,
                    Country = "Turkey"
                },

                BillingAddress = new Iyzipay.Model.Address
                {
                    ContactName = request.Name,
                    City = request.OrgAddress,
                    Country = "Turkey",
                    Description = "Fatura Adresi",
                    ZipCode = request.Zip
                },

                ShippingAddress = new Iyzipay.Model.Address
                {
                    ContactName = request.Name,
                    City = request.OrgAddress,
                    Country = "Turkey",
                    Description = "Teslimat Adresi",
                    ZipCode = request.Zip
                },

                BasketItems = new List<Iyzipay.Model.BasketItem>
                {
                    new Iyzipay.Model.BasketItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Abonelik Paketi",
                        Category1 = "Abonelik",
                        ItemType = Iyzipay.Model.BasketItemType.VIRTUAL.ToString(),
                        Price = priceInTRY.ToString("F2", CultureInfo.InvariantCulture)
                    }
                }
            };

            var payment = await Iyzipay.Model.Payment.Create(paymentRequest, _iyzicoOptions);

            if (payment.Status == "success")
            {
                var addplan = _userService.AddOrUpdatePlan(org.OrganizationId, priceUSD.Price, request.PlanId, request.IsYearly, true);
                var nextAmount = _userService.UpdateNextPaymentDatePlan(addplan, request.IsYearly);

                return Ok(payment);
            }
            else
            {
                return Ok(payment);
            }
        }

        [HttpPost("cancel-payment")]
        public async Task<IActionResult> CancelPayment()
        {
            var userId = UserId();
            var org = _userService.GetUserById(userId);
            var card = _userService.GetCard(org.OrganizationId);

            var _iyzicoOptions = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzico:ApiKey"],
                SecretKey = _configuration["Iyzico:SecretKey"],
                BaseUrl = _configuration["Iyzico:BaseUrl"],
            };

            if (card != null)
            {
                var deleteRequest = new DeleteCardRequest
                {
                    CardUserKey = card.CardUserKey,
                    CardToken = card.CardToken,
                    Locale = Locale.TR.ToString(),
                    ConversationId = org.OrganizationId.ToString()
                };

                var result = await Iyzipay.Model.Card.Delete(deleteRequest, _iyzicoOptions);
            }

            var deletePlan = _userService.DeletePlan(org.OrganizationId);

            return Ok(1);
        }
        #endregion

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

        private decimal GetCurrentUsdTryRate()
        {
            try
            {
                var xmlUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlUrl);

                var usdNode = xmlDoc.SelectSingleNode("//Currency[@CurrencyCode='USD']");

                var rateString = usdNode.SelectSingleNode("BanknoteSelling")?.InnerText;

                if (decimal.TryParse(rateString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rate))
                {
                    return rate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kur bilgisi alınamadı: " + ex.Message);
            }

            return 0m;
        }

        private decimal CalculatePriceInTRY(decimal priceInUSD)
        {
            var rate = GetCurrentUsdTryRate();
            return Math.Round(priceInUSD * rate, 2);
        }
    }
}
