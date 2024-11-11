using AdminPanel.Models.Meta.AdSet;
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
    public class MetaAdSetController : ControllerBase
    {
        private readonly ILogger<MetaAdSetController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;

        public MetaAdSetController(ILogger<MetaAdSetController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AdSetResponse>> GetAdSets(int userId)
        {
            MetaData metaData = new MetaData();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var adSets = metaData.AdSetsAdmin(accessToken.AccessToken, "342280538743641", "2024-01-01", "2024-11-08");
            var data = new AdSetResponse
            {
                Data = adSets.Data?.Select(q => new AdSet
                {
                    Id = q.Id,
                    Name = q.Name,
                    Status = q.Status,
                    BidStrategy = q.BidStrategy,
                    DailyBudget = q.DailyBudget,
                    LifeTimeBudget = q.LifeTimeBudget,
                    UpdateTime = q.UpdateTime,
                    StartTime = q.StartTime,
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
                }).ToList() ?? new List<AdSet>()
            };

            return Ok(new List<AdSetResponse> { data });
        }
    }
}
