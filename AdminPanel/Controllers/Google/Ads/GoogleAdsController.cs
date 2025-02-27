using AdminPanel.Controllers.Google.Analytics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V18.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google.Ads
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAdsController : ControllerBase
    {
        private readonly ILogger<GoogleAdsController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public GoogleAdsController(ILogger<GoogleAdsController> logger, GoogleService googleService, GoogleData googleData)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService, googleData);
            _googleData = googleData;
            _defaultValues = new DefaultValues();
        }

        //[HttpGet("account")]
        //public IActionResult GetAccount()
        //{
        //    var userId = UserId();
        //    var accessTokenControl = _googleTokenControl.GetControl(userId);
        //    GoogleAdsConfig config = new GoogleAdsConfig
        //    {
        //        OAuth2AccessToken = accessTokenControl
        //    };

        //    GoogleAdsClient client = new GoogleAdsClient(config);

        //    List<string> customerIds = new List<string>();

        //    CustomerServiceClient customerService = client.GetService(Services.V18.CustomerService);
        //    var accessibleAccounts = customerService.ListAccessibleCustomers();

        //    foreach (var account in accessibleAccounts)
        //    {
        //        string customerId = account.Split('/')[1];
        //        customerIds.Add(customerId);
        //    }

        //    return Ok(customerIds);
        //}

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
