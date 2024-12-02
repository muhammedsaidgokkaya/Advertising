using AdminPanel.Controllers.Google.SearchConsole;
using AdminPanel.Models.Google.Analytics.GeneralQuery;
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
    public class GoogleGeneralRateController : ControllerBase
    {
        private readonly ILogger<GoogleGeneralRateController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public GoogleGeneralRateController(ILogger<GoogleGeneralRateController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<GeneralRateResponse>> GetGeneralRateQuery(int userId, string property, string dimensions, DateTime? startDate = null, DateTime? endDate = null)
        {
            var propertyId = _defaultValues.GoogleProperty(property);
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 90);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var generalRateQuery = _googleData.GeneralRateAdmin(accessTokenControl, propertyId, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var generalRates = generalRateQuery.Select(gr => new GeneralRateResponse
            {
                Dimension = gr.Dimension,
                AverageSessionDuration = gr.AverageSessionDuration,
                EventsPerSession = gr.EventsPerSession,
                SessionKeyEventRate = gr.SessionKeyEventRate,
                ScreenPageViewsPerSession = gr.ScreenPageViewsPerSession,
                EngagementRate = gr.EngagementRate,
                EngagedSessions = gr.EngagedSessions,
                ScreenPageViewsPerUser = gr.ScreenPageViewsPerUser,
                EventCountPerUser = gr.EventCountPerUser,
                UserKeyEventRate = gr.UserKeyEventRate
            }).ToList();

            return Ok(generalRates);
        }
    }
}
