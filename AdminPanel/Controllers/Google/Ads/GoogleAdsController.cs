using AdminPanel.Controllers.Google.Analytics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.Util;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.V18.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Google.Ads.GoogleAds.V18.Enums;
using Google.Ads.GoogleAds.V18.Resources;
using Google.Api.Gax;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;
using Google.Ads.Gax.Config;
using Google.Ads.GoogleAds.V17.Errors;

namespace AdminPanel.Controllers.Google.Ads
{
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

        [HttpGet("account")]
        public IActionResult GetAccount()
        {
			var config = new GoogleAdsConfig()
			{
				DeveloperToken = "gP3mj269UEIGz2Nupz9N7w",
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = "876265473668-dkrg8ouj2qaginhpoamfdacf0f83002j.apps.googleusercontent.com",
				OAuth2ClientSecret = "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",
				OAuth2RefreshToken = "1//09Q9asmj-BAyXCgYIARAAGAkSNwF-L9IrZ8p-Qh6fDxn5ZN_qfS8XEva80Af0nf5nfPjHsjX9YkPD__LBre2U3P4tKMuD6I4WbH0"
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			var service = client.GetService(Services.V17.CustomerService);
			string[] customers = service.ListAccessibleCustomers();

			return Ok(customers);
        }

		[HttpGet("campaigns")]
		public IActionResult GetCampaigns()
		{
			var config = new GoogleAdsConfig()
			{
				DeveloperToken = "gP3mj269UEIGz2Nupz9N7w",
				OAuth2Mode = OAuth2Flow.APPLICATION,
				OAuth2ClientId = "876265473668-dkrg8ouj2qaginhpoamfdacf0f83002j.apps.googleusercontent.com",
				OAuth2ClientSecret = "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",
				OAuth2RefreshToken = "1//09Q9asmj-BAyXCgYIARAAGAkSNwF-L9IrZ8p-Qh6fDxn5ZN_qfS8XEva80Af0nf5nfPjHsjX9YkPD__LBre2U3P4tKMuD6I4WbH0"
			};

			GoogleAdsClient client = new GoogleAdsClient(config);

			string customerId = "5664228941";

			var service = client.GetService(Services.V18.GoogleAdsService);

			string query = @"
				SELECT
					campaign.id,
					campaign.name,
					campaign.status,
					campaign.start_date,
					campaign.end_date,
					campaign.advertising_channel_type
				FROM campaign";

			var searchRequest = new SearchGoogleAdsRequest()
			{
				CustomerId = customerId,
				Query = query,
				PageSize = 1000
			};

			var response = service.Search(searchRequest);

			var campaignList = new List<object>();

			foreach (var row in response)
			{
				campaignList.Add(new
				{
					Id = row.Campaign.Id,
					Name = row.Campaign.Name,
					Status = row.Campaign.Status.ToString(),
					StartDate = row.Campaign.StartDate,
					EndDate = row.Campaign.EndDate,
					ChannelType = row.Campaign.AdvertisingChannelType.ToString()
				});
			}

			return Ok(campaignList);
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
