using AdminPanel.Models;
using Core.Domain.Meta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using MetaAccess = AdminPanel.Models.MetaAccess;

namespace AdminPanel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private readonly ILogger<MetaController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;

        public MetaController(ILogger<MetaController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<MetaAccess>> GetMetaTokens(int id)
        {
            var data = _metaService.GetAccessToken(id).FirstOrDefault();
            var metaAccess = new MetaAccess
            {
                AccessToken = data.AccessToken,
                AppId = data.AppId,
                AppSecret = data.AppSecret,
                Name = data.User.Name
            };
            var longAccess = _metaService.GetLongLivedAccessTokenAsync(metaAccess.AppId, metaAccess.AppSecret, metaAccess.AccessToken);
            var model = _metaService.GetAccessToken(id);
            var list = model.Select(q => new MetaAccess
            {
                AccessToken = q.AccessToken,
                AppId = q.AppId,
                AppSecret = q.AppSecret,
                Name = q.User.Name
            });
            return list.ToList();
        }
    }
}
