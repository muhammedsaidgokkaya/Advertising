using AdminPanel.Controllers.Google.SearchConsole;
using AdminPanel.Models.Google.Analytics.Summary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google.Analytics
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAccountSummaryController : ControllerBase
    {
        private readonly ILogger<GoogleAccountSummaryController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;

        public GoogleAccountSummaryController(ILogger<GoogleAccountSummaryController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
        }

        [HttpGet]
        public ActionResult<IEnumerable<AccountSummaryResponse>> GetAccountSummary(int userId)
        {
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var accountSummary = _googleData.AccountSummaryAdmin(accessTokenControl);

            var data = new AccountSummaryResponse
            {
                AccountSummaries = accountSummary.AccountSummaries?.Select(q => new AccountSummary
                {
                    Name = q.Name,
                    Account = q.Account,
                    DisplayName = q.DisplayName,
                    PropertySummaries = q.PropertySummaries?.Select(i => new PropertySummary
                    {
                        Property = i.Property,
                        DisplayName = i.DisplayName,
                        PropertyType = i.PropertyType,
                        Parent = i.Parent,
                    }).ToList() ?? new List<PropertySummary>()
                }).ToList() ?? new List<AccountSummary>()
            };

            return Ok(new List<AccountSummaryResponse> { data });
        }
    }
}
