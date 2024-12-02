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
    public class GoogleDashboardDimensionController : ControllerBase
    {
        private readonly ILogger<GoogleDashboardDimensionController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public GoogleDashboardDimensionController(ILogger<GoogleDashboardDimensionController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<DashboardDimensionResponse>> GetDashboardDimension(int userId, string property, string dimension, string metric, DateTime? startDate = null, DateTime? endDate = null)
        {
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
    }
}
