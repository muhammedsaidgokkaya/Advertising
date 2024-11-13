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
        public ActionResult<IEnumerable<InsightGeneralResponse>> GetInsights(int userId, string accountId, DateTime? startDate = null, DateTime? endDate = null)
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
            var insights = metaData.InsightsAdmin(accessToken.AccessToken, accountId, defaultStartDate.ToString("yyyy-MM-dd"), defaultEndDate.ToString("yyyy-MM-dd"));
            var data = new InsightGeneralResponse
            {
                Data = insights.Data?.Select(q => new InsightGeneral
                {
                    Reach = q.Reach,
                    Frequency = q.Frequency,
                    Ctr = q.Ctr,
                    Impressions = q.Impressions,
                    Cpc = q.Cpc,
                    Cpm = q.Cpm,
                    Spend = q.Spend,
                    Clicks = q.Clicks,
                    DateStart = q.DateStart,
                    DateStop = q.DateStop,
                    OutboundClicks = q.OutboundClicks?.Select(action => new AdminPanel.Models.Meta.Action.Action
                    {
                        ActionType = action.ActionType,
                        Value = action.Value
                    }).ToList() ?? new List<Models.Meta.Action.Action>(),
                    OutboundClicksCtr = q.OutboundClicksCtr?.Select(action => new AdminPanel.Models.Meta.Action.Action
                    {
                        ActionType = action.ActionType,
                        Value = action.Value
                    }).ToList() ?? new List<Models.Meta.Action.Action>(),
                    Actions = q.Actions?.Select(action => new AdminPanel.Models.Meta.Action.Action
                    {
                        ActionType = action.ActionType,
                        Value = action.Value
                    }).ToList() ?? new List<Models.Meta.Action.Action>()
                }).ToList() ?? new List<InsightGeneral>()
            };

            return Ok(new List<InsightGeneralResponse> { data });
        }
    }
}
