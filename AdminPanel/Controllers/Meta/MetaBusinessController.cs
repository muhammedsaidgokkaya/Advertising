﻿using AdminPanel.Models.Meta.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Utilities.MetaData;

namespace AdminPanel.Controllers.Meta
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaBusinessController : ControllerBase
    {
        private readonly ILogger<MetaBusinessController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;

        public MetaBusinessController(ILogger<MetaBusinessController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<BusinessResponse>> GetBusiness(int userId)
        {
            MetaData metaData = new MetaData();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var business = metaData.BusinessAdmin(accessToken.AccessToken);
            var data = new BusinessResponse
            {
                Data = business.Data?.Select(q => new Business
                {
                    Id = q.Id,
                    Name = q.Name
                }).ToList() ?? new List<Business>()
            };

            return Ok(new List<BusinessResponse> { data });
        }
    }
}