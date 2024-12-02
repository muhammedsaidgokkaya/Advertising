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
    public class GoogleGeneralCountController : ControllerBase
    {
        private readonly ILogger<GoogleGeneralCountController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;
        private readonly DefaultValues _defaultValues;

        public GoogleGeneralCountController(ILogger<GoogleGeneralCountController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<GeneralCountResponse>> GetGeneralCountQuery(int userId, string property, string dimension, DateTime? startDate = null, DateTime? endDate = null)
        {
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
    }
}
