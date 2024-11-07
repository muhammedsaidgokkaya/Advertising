using AdminPanel.Models;
using Core.Domain.Meta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Utilities.MetaData;
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
        public ActionResult<IEnumerable<UserViewModel>> GetNewUser(int userId)
        {
            var model = _userService.GetById(userId);
            var personCount = _userService.GetAllCount();
            var list = model.Select(q => new UserViewModel
            {
                Id = q.Id,
                Name = q.Name,
                IsActive = q.IsActive,
                PersonCount = personCount
            });
            return list.ToList();
        }

        //test
        //[HttpGet]
        //public ActionResult<IEnumerable<UserViewModel>> GetNewUser(int userId)
        //{
        //    var shortAccessToken = _metaService.GetAccessToken(userId);
        //    MetaData metaData = new MetaData();
        //    var longAccessToken = metaData.LongAccessTokenAdmin(shortAccessToken.AppId, shortAccessToken.AppSecret, shortAccessToken.AccessToken);
        //    var longAccesTokenUserId = _metaService.AddLongAccessToken(shortAccessToken.Id, longAccessToken.AccessToken, longAccessToken.TokenType, longAccessToken.ExpiresIn);
        //    var id = 1;
        //    var model = _userService.GetById(id);
        //    var personCount = _userService.GetAllCount();
        //    var list = model.Select(q => new UserViewModel
        //    {
        //        Id = q.Id,
        //        Name = q.Name,
        //        IsActive = q.IsActive,
        //        PersonCount = personCount
        //    });
        //    return list.ToList();
        //}
    }
}
