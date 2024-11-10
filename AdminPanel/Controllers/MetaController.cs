using AdminPanel.Models;
using Core.Domain.Meta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Utilities.MetaData;

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

        //[HttpGet]
        //public ActionResult<IEnumerable<UserViewModel>> GetNewUser(int id)
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

        //test
        [HttpGet]
        public ActionResult<IEnumerable<CampaignAdminViewModel.CampaignResponse>> GetCampaigns(int userId)
        {
            MetaData metaData = new MetaData();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var campaigns = metaData.CampaignsAdmin(accessToken.AccessToken, "342280538743641", "2024-01-01", "2024-11-08");
            var data = new CampaignAdminViewModel.CampaignResponse
            {
                Data = campaigns.Data?.Select(q => new CampaignAdminViewModel.Campaign
                {
                    Id = q.Id,
                    Name = q.Name,
                    Status = q.Status,
                    AccountId = q.AccountId,
                    EndTime = q.EndTime,
                    Insights = new CampaignAdminViewModel.InsightResponse
                    {
                        Data = q.Insights?.Data?.Select(i => new CampaignAdminViewModel.Insight
                        {
                            Reach = i.Reach,
                            Impressions = i.Impressions,
                            Cpc = i.Cpc,
                            Cpm = i.Cpm,
                            Spend = i.Spend,
                            DateStart = i.DateStart,
                            DateStop = i.DateStop,
                            Actions = i.Actions?.Select(action => new CampaignAdminViewModel.Action
                            {
                                ActionType = action.ActionType,
                                Value = action.Value
                            }).ToList() ?? new List<CampaignAdminViewModel.Action>()
                        }).ToList() ?? new List<CampaignAdminViewModel.Insight>()
                    }
                }).ToList() ?? new List<CampaignAdminViewModel.Campaign>()
            };

            return Ok(new List<CampaignAdminViewModel.CampaignResponse> { data });
        }
    }
}
