using AdminPanel.Models.Google.Analytics.Dashboard;
using AdminPanel.Models.Google.Analytics.GeneralQuery;
using AdminPanel.Models.Google.Analytics.Summary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Google;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.GoogleData;

namespace AdminPanel.Controllers.Google.Analytics
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ILogger<AnalyticsController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public AnalyticsController(ILogger<AnalyticsController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
            _defaultValues = new DefaultValues();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("account-summarys")]
        public ActionResult<IEnumerable<AccountSummaryResponse>> GetAccountSummary()
        {
            var userId = UserId();
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

        [HttpGet("dashboards")]
        public ActionResult<IEnumerable<DashboardResponse>> GetDashboard(string property, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
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

        [HttpGet("dashboard-dimensions")]
        public ActionResult<IEnumerable<DashboardDimensionResponse>> GetDashboardDimension(string property, string dimension, string metric, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var propertyId = _defaultValues.GoogleProperty(property);
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 90);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var dashboardDimension = _googleData.DashboardDimensionAdmin(accessTokenControl, propertyId, dimension, metric, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var dashboardDimensions = dashboardDimension.Select(gr => new DashboardDimensionResponse
            {
                Dimension = gr.Dimension,
                Metric = gr.Metric,
            }).ToList();

            return Ok(dashboardDimensions);
        }

        [HttpGet("count-query")]
        public ActionResult<IEnumerable<GeneralCountResponse>> GetGeneralCountQuery(string property, string dimension, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var propertyId = _defaultValues.GoogleProperty(property);
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 90);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var generalCountQuery = _googleData.GeneralCountAdmin(accessTokenControl, propertyId, dimension, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var generalCounts = generalCountQuery.Select(gr => new GeneralCountResponse
            {
                Dimension = gr.Dimension,
                TotalUsers = gr.TotalUsers,
                ActiveUsers = gr.ActiveUsers,
                NewUsers = gr.NewUsers,
                ScreenPageViews = gr.ScreenPageViews,
                Sessions = gr.Sessions,
                EventCount = gr.EventCount,
                KeyEvents = gr.KeyEvents,
                TotalRevenue = gr.TotalRevenue,
                Transactions = gr.Transactions,
            }).ToList();

            return Ok(generalCounts);
        }

        [HttpGet("rate-query")]
        public ActionResult<IEnumerable<GeneralRateResponse>> GetGeneralRateQuery(string property, string dimensions, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
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
