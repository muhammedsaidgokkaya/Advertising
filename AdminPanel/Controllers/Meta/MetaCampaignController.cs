using AdminPanel.Models.Meta.Campaign;
using AdminPanel.Models.Meta.Insight;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Utilities.MetaData;

namespace AdminPanel.Controllers.Meta
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaCampaignController : ControllerBase
    {
        private readonly ILogger<MetaCampaignController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;

        public MetaCampaignController(ILogger<MetaCampaignController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CampaignResponse>> GetCampaigns(int userId, string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime defaultEndDate = endDate ?? DateTime.Now;
            DateTime defaultStartDate = startDate ?? defaultEndDate.AddDays(-30);
            if (startDate.HasValue && !endDate.HasValue)
            {
                defaultEndDate = startDate.Value.AddDays(1);
            }
            else if (!startDate.HasValue && endDate.HasValue)
            {
                defaultStartDate = endDate.Value.AddDays(-1);
            }
            MetaData metaData = new MetaData();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var campaigns = metaData.CampaignsAdmin(accessToken.AccessToken, accountId, defaultStartDate.ToString("yyyy-MM-dd"), defaultEndDate.ToString("yyyy-MM-dd"));
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
