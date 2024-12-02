using AdminPanel.Models.Meta.Insight;
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
    public class MetaController : ControllerBase
    {
        private readonly ILogger<MetaController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;
        private readonly MetaData _metaData;
        private readonly DefaultValues _defaultValues;

        public MetaController(ILogger<MetaController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
            _metaData = new MetaData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<InsightGeneralResponse>> GetInsights(int userId, string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate, 90);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var insights = _metaData.InsightsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
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
