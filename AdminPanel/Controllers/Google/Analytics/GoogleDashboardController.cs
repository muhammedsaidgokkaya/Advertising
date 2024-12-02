using AdminPanel.Models.Google.Analytics.Dashboard;
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
    public class GoogleDashboardController : ControllerBase
    {
        private readonly ILogger<GoogleDashboardController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public GoogleDashboardController(ILogger<GoogleDashboardController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<DashboardResponse>> GetDashboard(int userId, string property, DateTime? startDate = null, DateTime? endDate = null)
        {
            var propertyId = _defaultValues.GoogleProperty(property);
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 90);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var dashboard = _googleData.DashboardAdmin(accessTokenControl, propertyId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var dashboards = dashboard.Select(gr => new DashboardResponse
            {
                ActiveUsers = gr.ActiveUsers,
                EventCount = gr.EventCount,
                EngagedSessions = gr.EngagedSessions,
                NewUsers = gr.NewUsers,
            }).ToList();

            return Ok(dashboards);
        }
    }
}
