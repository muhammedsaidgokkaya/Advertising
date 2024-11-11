using AdminPanel.Models;
using AdminPanel.Models.Meta.Campaign;
using AdminPanel.Models.Meta.Insight;
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

        //test
        [HttpGet]
        public ActionResult<IEnumerable<CampaignResponse>> GetCampaigns(int userId)
        {
            MetaData metaData = new MetaData();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var campaigns = metaData.CampaignsAdmin(accessToken.AccessToken, "342280538743641", "2024-01-01", "2024-11-08");
            var data = new CampaignResponse
            {
                Data = campaigns.Data?.Select(q => new Campaign
                {
                    Id = q.Id,
                    Name = q.Name,
                    Status = q.Status,
                    AccountId = q.AccountId,
                    EndTime = q.EndTime,
                    Insights = new InsightResponse
                    {
                        Data = q.Insights?.Data?.Select(i => new Insight
                        {
                            Reach = i.Reach,
                            Impressions = i.Impressions,
                            Cpc = i.Cpc,
                            Cpm = i.Cpm,
                            Spend = i.Spend,
                            DateStart = i.DateStart,
                            DateStop = i.DateStop,
                            Actions = i.Actions?.Select(action => new AdminPanel.Models.Meta.Action.Action
                            {
                                ActionType = action.ActionType,
                                Value = action.Value
                            }).ToList() ?? new List<Models.Meta.Action.Action>()
                        }).ToList() ?? new List<Insight>()
                    }
                }).ToList() ?? new List<Campaign>()
            };

            return Ok(new List<CampaignResponse> { data });
        }
    }
}
