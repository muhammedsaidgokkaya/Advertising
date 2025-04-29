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

        public AnalyticsController(ILogger<AnalyticsController> logger, GoogleService googleService, GoogleData googleData)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService, googleData);
            _googleData = googleData;
            _defaultValues = new DefaultValues();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("account-summarys")]
        public ActionResult<IEnumerable<AccountSummaryResponse>> GetAccountSummary()
        {
            var userId = UserId();
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
			var analyticsAccount = organization.GoogleAnalytics ?? string.Empty;
			var result = analyticsAccount
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(accountInfo =>
				{
					var parts = accountInfo.Split('/');
					return new
                    {
                        account = parts[0],
                        accountId = parts[1] + "/" + parts[2]
					};
				})
				.ToList();

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

			var accountIds = result.Select(a => a.accountId).ToHashSet();
			var availableAccounts = data.AccountSummaries
				.Where(a => !a.PropertySummaries
		        .Any(p => accountIds.Contains(p.Property)))
	            .ToList();

			var selectedAccounts = data.AccountSummaries
				.Where(a => a.PropertySummaries
				.Any(p => accountIds.Contains(p.Property)))
				.ToList();

			var responses = new
			{
				Available = availableAccounts,
				Selected = selectedAccounts
			};

			return Ok(responses);
		}

        [HttpGet("analytics-account")]
        public ActionResult<IEnumerable<object>> GetOrganizationAnalyticsAccount()
        {
            var userId = UserId();
            var user = _userService.GetUserById(userId);
            var organization = _userService.GetOrganizationById(user.OrganizationId);
            var analyticsAccount = organization.GoogleAnalytics;
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

            return Ok(result);
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<IEnumerable<DashboardResponse>>> GetDashboard()
        {
            var userId = UserId();
			var user = _userService.GetUserById(userId);
			var organization = _userService.GetOrganizationById(user.OrganizationId);
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
						accountId = parts[1] + "/" + parts[2]
					};
				})
				.ToList();
				var accountIds = result.Select(r => r.accountId).ToList();
				var accessTokenControl = _googleTokenControl.GetControl(userId);
				var dashboard = await _googleData.Dashboards(accessTokenControl, accountIds);
				return Ok(dashboard);
			}
            return Ok(0);
        }

        [HttpGet("dashboards")]
        public async Task<ActionResult<IEnumerable<DashboardResponse>>> GetDashboards(string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var dashboard = await _googleData.DashboardAdmin(accessTokenControl, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var dashboards = dashboard.Select(group => new DashboardResponse
            {
                Month = group.Month,
                Data = group.Data != null && group.Data.Any()
                    ? group.Data.Select(g => new Dashboard
                    {
                        ActiveUsers = g.ActiveUsers == 0 ? 0 : g.ActiveUsers,
                        EventCount = g.EventCount == 0 ? 0 : g.EventCount,
                        NewUsers = g.NewUsers == 0 ? 0 : g.NewUsers,
                        EngagedSessions = g.EngagedSessions == 0 ? 0 : g.EngagedSessions
                    }).ToList()
                    : new List<Dashboard>
                    {
                        new Dashboard
                        {
                            ActiveUsers = 0,
                            EventCount = 0,
                            NewUsers = 0,
                            EngagedSessions = 0
                        }
                    }
            }).ToList();

            return Ok(dashboards);
        }

        [HttpGet("dashboard-dimensions-four")]
        public async Task<ActionResult<IEnumerable<DashboardDimensionResponse>>> GetDashboardDimensionFour(string accountId, string dimension, string metric, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var dashboardDimension = await _googleData.DashboardDimensionAdmin(accessTokenControl, accountId, dimension, metric, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var dashboardDimensions = dashboardDimension.Take(4).Select(gr => new DashboardDimensionResponse
            {
                Dimension = gr.Dimension == "(not set)" ? "Bilinmeyen" : gr.Dimension,
                Metric = gr.Metric,
            }).ToList();

            return Ok(dashboardDimensions);
        }

        [HttpGet("dashboard-dimensions-ten")]
        public async Task<ActionResult<IEnumerable<DashboardDimensionResponse>>> GetDashboardDimensionTen(string accountId, string dimension, string metric, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var dashboardDimension = await _googleData.DashboardDimensionAdmin(accessTokenControl, accountId, dimension, metric, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));

            var dashboardDimensions = dashboardDimension.Take(10).Select(gr => new DashboardDimensionResponse
            {
                Dimension = gr.Dimension == "(not set)" ? "Bilinmeyen" : gr.Dimension,
                Metric = gr.Metric,
            }).ToList();

            return Ok(dashboardDimensions);
        }

        [HttpGet("query")]
        public async Task<ActionResult<IEnumerable<GeneralRateResponse>>> GetGeneralQuery(string accountId, string dimensions, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = UserId();
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 120);
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var generalRateQuery = await _googleData.GeneralRateAdmin(accessTokenControl, accountId, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var generalRates = generalRateQuery.Select(gr => new GeneralRateResponse
            {
                Dimension = gr.Dimension == "(not set)" ? "Bilinmeyen" : gr.Dimension,
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

            var generalCountQuery = await _googleData.GeneralCountAdmin(accessTokenControl, accountId, dimensions, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var generalCounts = generalCountQuery.Select(gc => new GeneralCountResponse
            {
                Dimension = gc.Dimension == "(not set)" ? "Bilinmeyen" : gc.Dimension,
                TotalUsers = gc.TotalUsers,
                ActiveUsers = gc.ActiveUsers,
                NewUsers = gc.NewUsers,
                ScreenPageViews = gc.ScreenPageViews,
                Sessions = gc.Sessions,
                EventCount = gc.EventCount,
                KeyEvents = gc.KeyEvents,
                TotalRevenue = gc.TotalRevenue,
                Transactions = gc.Transactions
            }).ToList();

            var combinedData = (from rate in generalRates
                                join count in generalCounts
                                on rate.Dimension equals count.Dimension
                                select new CombinedRateCountResponse
                                {
                                    Dimension = rate.Dimension,
                                    AverageSessionDuration = rate.AverageSessionDuration,
                                    EventsPerSession = rate.EventsPerSession,
                                    SessionKeyEventRate = rate.SessionKeyEventRate,
                                    ScreenPageViewsPerSession = rate.ScreenPageViewsPerSession,
                                    EngagementRate = rate.EngagementRate,
                                    EngagedSessions = rate.EngagedSessions,
                                    ScreenPageViewsPerUser = rate.ScreenPageViewsPerUser,
                                    EventCountPerUser = rate.EventCountPerUser,
                                    UserKeyEventRate = rate.UserKeyEventRate,
                                    TotalUsers = count.TotalUsers,
                                    ActiveUsers = count.ActiveUsers,
                                    NewUsers = count.NewUsers,
                                    ScreenPageViews = count.ScreenPageViews,
                                    Sessions = count.Sessions,
                                    EventCount = count.EventCount,
                                    KeyEvents = count.KeyEvents,
                                    TotalRevenue = count.TotalRevenue,
                                    Transactions = count.Transactions
                                }).ToList();

            var resultWithIds = combinedData.Select((data, index) =>
            {
                data.Id = index + 1;
                return data;
            }).ToList();

            return Ok(resultWithIds);
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
