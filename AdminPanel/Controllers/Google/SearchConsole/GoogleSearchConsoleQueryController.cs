using AdminPanel.Models.Google.SearchConsole.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google.SearchConsole
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleSearchConsoleQueryController : ControllerBase
    {
        private readonly ILogger<GoogleSearchConsoleQueryController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public GoogleSearchConsoleQueryController(ILogger<GoogleSearchConsoleQueryController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<RowResponse>> GetSearchConsoleQuery(int userId, string url, string dimensions, string rows = "10", DateTime? startDate = null, DateTime? endDate = null)
        {
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var searchConsoleQuery = _googleData.SearchConsoleQueryAdmin(accessTokenControl, url, rows, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

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
