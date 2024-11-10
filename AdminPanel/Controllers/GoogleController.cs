using AdminPanel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Meta;
using Service.Implementations.User;
using Utilities.Utilities.MetaData;

namespace AdminPanel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly ILogger<GoogleController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;

        public GoogleController(ILogger<GoogleController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AdSetViewModel.AdSetResponse>> GetAdSets(int userId)
        {
            MetaData metaData = new MetaData();
            var accessToken = _metaService.GetLongAccessToken(userId);
            var adSets = metaData.AdSetsAdmin(accessToken.AccessToken, "342280538743641", "2024-01-01", "2024-11-08");
            var data = new AdSetViewModel.AdSetResponse
            {
                Data = adSets.Data?.Select(q => new AdSetViewModel.AdSet
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
                    Insights = new AdSetViewModel.InsightResponse
                    {
                        Data = q.Insights?.Data?.Select(i => new AdSetViewModel.Insight
                        {
                            Reach = i.Reach,
                            Impressions = i.Impressions,
                            Cpc = i.Cpc,
                            Cpm = i.Cpm,
                            Spend = i.Spend,
                            DateStart = i.DateStart,
                            DateStop = i.DateStop,
                            Actions = i.Actions?.Select(action => new AdSetViewModel.Action
                            {
                                ActionType = action.ActionType,
                                Value = action.Value
                            }).ToList() ?? new List<AdSetViewModel.Action>()
                        }).ToList() ?? new List<AdSetViewModel.Insight>()
                    }
                }).ToList() ?? new List<AdSetViewModel.AdSet>()
            };

            return Ok(new List<AdSetViewModel.AdSetResponse> { data });
        }
    }
}
