using Google.Apis.Auth.OAuth2;
using Google.Apis.SearchConsole.v1.Data;
using Google.Apis.SearchConsole.v1;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Utilities.GoogleData.SearchConsole
{
    public class SearchConsoleMetrics
    {
        public int TotalClicks { get; set; }
        public int TotalImpressions { get; set; }
        public double AverageCtr { get; set; }
        public double AveragePosition { get; set; }
        public double ClicksChange { get; set; }
        public double ImpressionsChange { get; set; }
        public double CtrChange { get; set; }
        public double PositionChange { get; set; }
    }

    public class SearchConsole
    {
        public SearchConsoleService CreateSearchConsoleService(string accessToken)
        {
            var credential = GoogleCredential.FromAccessToken(accessToken);
            var service = new SearchConsoleService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Search Console API",
            });
            return service;
        }

        public Dictionary<string, object> GetGoogleSearchConsoleMetrics(SearchConsoleService service, string siteUrl, string startDate, string endDate)
        {
            var request = service.Searchanalytics.Query(new SearchAnalyticsQueryRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                RowLimit = 1
            }, siteUrl);

            var response = request.Execute();

			var row = response.Rows?.FirstOrDefault();

			if (row == null)
			{
				return new Dictionary<string, object>
		        {
			        { "total_clicks", 0 },
			        { "total_impressions", 0 },
			        { "average_ctr", 0 },
			        { "average_position", 0 }
		        };
			}

			var metrics = new Dictionary<string, object>
	        {
		        { "total_clicks", row.Clicks ?? 0 },
		        { "total_impressions", row.Impressions ?? 0 },
		        { "average_ctr", row.Ctr ?? 0 },
		        { "average_position", row.Position ?? 0 }
	        };

			return metrics;
        }

        public double CalculateChange(double previous, double current)
        {
            if (previous == 0)
            {
                return current == 0 ? 0 : 0;
            }
            return (current - previous) / previous * 100;
        }

        public SearchConsoleMetrics GetSearchConsoleMetrics(string accessToken, string siteUrl, string startDate, string endDate)
        {
            var service = CreateSearchConsoleService(accessToken);

            var currentMetrics = GetGoogleSearchConsoleMetrics(service, siteUrl, startDate, endDate);

            var startDateObj = DateTime.Parse(startDate);
            var endDateObj = DateTime.Parse(endDate);
            var delta = endDateObj - startDateObj;
            var previousStartDate = startDateObj.AddDays(-delta.Days).ToString("yyyy-MM-dd");
            var previousEndDate = endDateObj.AddDays(-delta.Days).ToString("yyyy-MM-dd");

            var previousMetrics = GetGoogleSearchConsoleMetrics(service, siteUrl, previousStartDate, previousEndDate);

            var clicksChange = CalculateChange(Convert.ToDouble(previousMetrics["total_clicks"]), Convert.ToDouble(currentMetrics["total_clicks"]));
            var impressionsChange = CalculateChange(Convert.ToDouble(previousMetrics["total_impressions"]), Convert.ToDouble(currentMetrics["total_impressions"]));
            var ctrChange = CalculateChange(Convert.ToDouble(previousMetrics["average_ctr"]), Convert.ToDouble(currentMetrics["average_ctr"]));
            var positionChange = CalculateChange(Convert.ToDouble(previousMetrics["average_position"]), Convert.ToDouble(currentMetrics["average_position"]));

            var result = new SearchConsoleMetrics
            {
                TotalClicks = Convert.ToInt32(currentMetrics["total_clicks"]),
                TotalImpressions = Convert.ToInt32(currentMetrics["total_impressions"]),
                AverageCtr = Convert.ToDouble(currentMetrics["average_ctr"]),
                AveragePosition = Convert.ToDouble(currentMetrics["average_position"]),
                ClicksChange = clicksChange,
                ImpressionsChange = impressionsChange,
                CtrChange = ctrChange,
                PositionChange = positionChange
            };

            return result;
        }
    }
}
