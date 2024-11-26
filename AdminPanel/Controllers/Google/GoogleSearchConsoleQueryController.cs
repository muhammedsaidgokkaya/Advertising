using AdminPanel.Models.Google.SearchConsole.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleSearchConsoleQueryController : ControllerBase
    {
        private readonly ILogger<GoogleSearchConsoleQueryController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;

        public GoogleSearchConsoleQueryController(ILogger<GoogleSearchConsoleQueryController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<RowResponse>> GetSiteMaps(int userId, string url, string dimensions, string rows = "10", DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime defaultEndDate = endDate ?? DateTime.Now;
            DateTime defaultStartDate = startDate ?? defaultEndDate.AddDays(-30);
            if (startDate.HasValue && !endDate.HasValue)
            {
                defaultEndDate = startDate.Value.AddDays(1);
            }
            else if (!startDate.HasValue && endDate.HasValue)
            {
                defaultStartDate = endDate.Value.AddDays(-1);
            }

            var control = _googleService.GetGoogleAccessToken(userId);
            GoogleData googleData = new GoogleData();
            if (control == null)
            {
                var model = _googleService.GetGoogleApp(userId);
                var accessToken = _googleService.GetGoogleAccessTokenControl(userId);
                var refreshToken = googleData.RefreshAccessTokenAdmin(model.AppId, model.AppSecret, accessToken.RefreshToken);
                var newAccessToken = _googleService.AddGoogleAccessToken(model.Id, userId, refreshToken.AccessToken, accessToken.RefreshToken, refreshToken.ExpiresIn, refreshToken.Scope, refreshToken.TokenType);
                control = _googleService.GetGoogleAccessToken(userId);
            }
            var searchConsoleQuery = googleData.SearchConsoleQueryAdmin(control.AccessToken, url, rows, dimensions, defaultStartDate.ToString("yyyy-MM-dd"), defaultEndDate.ToString("yyyy-MM-dd"));

            var data = new RowResponse
            {
                Rows = searchConsoleQuery.Rows?.Select(s => new Row
                {
                    Keys = s.Keys,
                    Clicks = s.Clicks,
                    Impressions = s.Impressions,
                    Ctr = s.Ctr,
                    Position = s.Position
                }).ToList() ?? new List<Row>(),
                ResponseAggregationType = searchConsoleQuery.ResponseAggregationType
            };

            return Ok(new List<RowResponse> { data });
        }
    }
}
