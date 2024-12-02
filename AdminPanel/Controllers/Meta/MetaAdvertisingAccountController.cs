using AdminPanel.Models.Meta.AdvertisingAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Helper;
using Utilities.Utilities.MetaData;

namespace AdminPanel.Controllers.Meta
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaAdvertisingAccountController : ControllerBase
    {
        private readonly ILogger<MetaAdvertisingAccountController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;
        private readonly MetaData _metaData;

        public MetaAdvertisingAccountController(ILogger<MetaAdvertisingAccountController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
            _metaData = new MetaData();
        }

        [HttpGet]
        public ActionResult<IEnumerable<AdvertisingAccountsResponse>> GetAdvertisingAccounts(int userId, string businessId)
        {
            var accessToken = _metaService.GetLongAccessToken(userId);
            var advertisingAccount = _metaData.AdvertisingAccountsAdmin(accessToken.AccessToken, businessId);
            var data = new AdvertisingAccountsResponse
            {
                Data = advertisingAccount.Data?.Select(q => new AdvertisingAccount
                {
                    Id = q.Id,
                    Name = q.Name
                }).ToList() ?? new List<AdvertisingAccount>()
            };

            return Ok(new List<AdvertisingAccountsResponse> { data });
        }
    }
}
