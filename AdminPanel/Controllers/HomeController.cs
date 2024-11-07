using AdminPanel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.User;
using static NpgsqlTypes.NpgsqlTsVector;

namespace AdminPanel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserService _userService;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _userService = new UserService();
        }

        //[HttpGet]
        //public ActionResult<IEnumerable<UserViewModel>> GetUsers(int id)
        //{
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

        //[HttpGet]
        //public ActionResult<IEnumerable<UserViewModel>> GetError()
        //{
        //    MetaData metaData = new MetaData();
        //    var result = metaData.deneme("dsacsa", "1254545");
        //    return Ok(result);
        //}

        //[HttpGet]
        //public ActionResult<IEnumerable<UserViewModel>> GetNewUser(string signed_request, string graph_domain, string access_token, long data_access_expiration_time, long expires_in)
        //{
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

        [HttpGet]
        public ActionResult<IEnumerable<UserViewModel>> GetNewUser(int userId)
        {
            var id = 1;
            var model = _userService.GetById(id);
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
    }
}
