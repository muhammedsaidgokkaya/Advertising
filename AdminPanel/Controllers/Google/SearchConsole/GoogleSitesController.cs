using AdminPanel.Models.Google.SearchConsole.Site;
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
    public class GoogleSitesController : ControllerBase
    {
        private readonly ILogger<GoogleSitesController> _logger;
        private readonly UserService _userService;
        private readonly GoogleService _googleService;
        private readonly GoogleTokenControl _googleTokenControl;
        private readonly GoogleData _googleData;

        public GoogleSitesController(ILogger<GoogleSitesController> logger, GoogleService googleService)
        {
            _logger = logger;
            _userService = new UserService();
            _googleService = googleService;
            _googleTokenControl = new GoogleTokenControl(googleService);
            _googleData = new GoogleData();
        }

        [HttpGet]
        public ActionResult<IEnumerable<SiteResponse>> GetSites(int userId)
        {
            var accessTokenControl = _googleTokenControl.GetControl(userId);
            var sites = _googleData.SiteAdmin(accessTokenControl);

            var data = new SiteResponse
            {
                SiteEntry = sites.SiteEntry?.Select(q => new Sites
                {
                    SiteUrl = q.SiteUrl,
                    PermissionLevel = q.PermissionLevel
                }).ToList() ?? new List<Sites>()
            };

            return Ok(new List<SiteResponse> { data });
        }
    }
}
