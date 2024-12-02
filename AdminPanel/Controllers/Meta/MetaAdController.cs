using AdminPanel.Models.Meta.Ad;
using AdminPanel.Models.Meta.AdSet;
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
    public class MetaAdController : ControllerBase
    {
        private readonly ILogger<MetaAdController> _logger;
        private readonly UserService _userService;
        private readonly MetaService _metaService;
        private readonly MetaData _metaData;
        private readonly DefaultValues _defaultValues;

        public MetaAdController(ILogger<MetaAdController> logger, MetaService metaService)
        {
            _logger = logger;
            _userService = new UserService();
            _metaService = metaService;
            _metaData = new MetaData();
            _defaultValues = new DefaultValues();
        }

        [HttpGet]
        public ActionResult<IEnumerable<AdResponse>> GetAds(int userId, string accountId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var defaultValues = _defaultValues.DefaultDate(startDate, endDate);
            var accessToken = _metaService.GetLongAccessToken(userId);
            var ads = _metaData.AdsAdmin(accessToken.AccessToken, accountId, defaultValues[0].ToString("yyyy-MM-dd"), defaultValues[1].ToString("yyyy-MM-dd"));
            var data = new AdResponse
            {
                Data = ads.Data?.Select(q => new Ad
                {
                    Name = q.Name,
                    Status = q.Status,
                    AdSet = new AdSet
                    {
                        Name = q.AdSet.Name,
                        BidStrategy = q.AdSet.BidStrategy,
                        DailyBudget = q.AdSet.DailyBudget,
                        UpdateTime = q.AdSet.UpdateTime,
                    },
                    Insights = new InsightResponse
                    {
                        Data = q.Insights?.Data?.Select(i => new Insight
                        {
                            Reach = i.Reach,
                            Impressions = i.Impressions,
                            Cpc = i.Cpc,
                            Cpm = i.Cpm,
                            Spend = i.Spend,
                            ConversionRateRanking = i.ConversionRateRanking,
                            EngagementRateRanking = i.EngagementRateRanking,
                            QualityRanking = i.QualityRanking,
                            DateStart = i.DateStart,
                            DateStop = i.DateStop,
                            Actions = i.Actions?.Select(action => new AdminPanel.Models.Meta.Action.Action
                            {
                                ActionType = action.ActionType,
                                Value = action.Value
                            }).ToList() ?? new List<Models.Meta.Action.Action>()
                        }).ToList() ?? new List<Insight>()
                    }
                }).ToList() ?? new List<Ad>()
            };

            return Ok(new List<AdResponse> { data });
        }
    }
}
