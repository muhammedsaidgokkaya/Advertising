using Google.Apis.AnalyticsData.v1beta;
using Google.Apis.AnalyticsData.v1beta.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.GoogleAnalyticsAdmin.v1beta;
using Google.Apis.GoogleAnalyticsAdmin.v1beta.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.Utilities.GoogleData.GoogleData;

namespace Utilities.Utilities.GoogleData.Analytics
{
	public class Analytics
	{
		public AccountSummaryResponse AccountSummaryAdmin(string accessToken)
		{
			var credential = GoogleCredential.FromAccessToken(accessToken);

			var service = new GoogleAnalyticsAdminService(new BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "AnalyticsAdminSample"
			});

			var request = service.AccountSummaries.List();
			var response = request.Execute();

			var summaries = response.AccountSummaries?.Select(a => new AccountSummary
			{
				Name = a.Name,
				Account = a.Account,
				DisplayName = a.DisplayName,
				PropertySummaries = a.PropertySummaries?.Select(p => new PropertySummary
				{
					Property = p.Property,
					DisplayName = p.DisplayName,
					PropertyType = p.PropertyType,
					Parent = p.Parent
				}).ToList()
			}).ToList();

			return new AccountSummaryResponse
			{
				AccountSummaries = summaries
			};
		}

		public async Task<List<DashboardResponse>> GetAnalyticsDashboardMonthly(string accessToken, string propertyId, string startDateStr, string endDateStr)
		{
			var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);

			var service = new Google.Apis.AnalyticsData.v1beta.AnalyticsDataService(new Google.Apis.Services.BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "AnalyticsDataSample"
			});

			DateTime start = DateTime.ParseExact(startDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
			DateTime end = DateTime.ParseExact(endDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
			DateTime current = start;

			var results = new List<DashboardResponse>();

			while (current <= end)
			{
				DateTime monthStart = new DateTime(current.Year, current.Month, 1);
				DateTime nextMonth = monthStart.AddMonths(1);
				DateTime monthEnd = nextMonth.AddDays(-1);
				if (monthEnd > end) monthEnd = end;

				var requestBody = new RunReportRequest
				{
					Metrics = new List<Metric>
					{
						new Metric { Name = "activeUsers" },
						new Metric { Name = "eventCount" },
						new Metric { Name = "newUsers" },
						new Metric { Name = "engagedSessions" }
					},
					DateRanges = new List<DateRange>
					{
						new DateRange
						{
							StartDate = monthStart.ToString("yyyy-MM-dd"),
							EndDate = monthEnd.ToString("yyyy-MM-dd")
						}
					}
				};

				var request = service.Properties.RunReport(requestBody, $"properties/{propertyId}");
				var response = await request.ExecuteAsync();

				var metricList = new List<Dashboard>();

				if (response.Rows != null)
				{
					foreach (var row in response.Rows)
					{
						var data = new Dashboard
						{
							ActiveUsers = int.TryParse(row.MetricValues.ElementAtOrDefault(0)?.Value, out var activeUsers) ? activeUsers : 0,
							EventCount = int.TryParse(row.MetricValues.ElementAtOrDefault(1)?.Value, out var eventCount) ? eventCount : 0,
							NewUsers = int.TryParse(row.MetricValues.ElementAtOrDefault(2)?.Value, out var newUsers) ? newUsers : 0,
							EngagedSessions = int.TryParse(row.MetricValues.ElementAtOrDefault(3)?.Value, out var engagedSessions) ? engagedSessions : 0
						};
						metricList.Add(data);
					}
				}

				results.Add(new DashboardResponse
				{
					Month = monthStart.ToString("yyyy-MM"),
					Data = metricList
				});

				current = nextMonth;
			}

			return results;
		}

		public async Task<int> GetTotalActiveUsers(string accessToken)
		{
			var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);

			var analyticsAdminService = new GoogleAnalyticsAdminService(
				new BaseClientService.Initializer
				{
					HttpClientInitializer = credential,
					ApplicationName = "Analytics Admin API Sample"
				});

			var analyticsDataService = new Google.Apis.AnalyticsData.v1beta.AnalyticsDataService(
				new Google.Apis.Services.BaseClientService.Initializer
				{
					HttpClientInitializer = credential,
					ApplicationName = "AnalyticsDataSample"
				});

			var adminRequest = analyticsAdminService.AccountSummaries.List();
			var adminResponse = await adminRequest.ExecuteAsync();

			var propertyIds = adminResponse.AccountSummaries?
				.SelectMany(a => a.PropertySummaries ?? Enumerable.Empty<GoogleAnalyticsAdminV1betaPropertySummary>())
				.Select(p => p.Property)
				.ToList();

			var totalActiveUsers = 0;
			var startDate = "2015-08-14";
			var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

			foreach (var propertyId in propertyIds)
			{
				var requestBody = new RunReportRequest
				{
					Metrics = new List<Metric>
					{
						new Metric { Name = "activeUsers" }
					},
					DateRanges = new List<DateRange>
					{
						new DateRange
						{
							StartDate = startDate,
							EndDate = endDate
						}
					}
				};

				var analyticsRequest = analyticsDataService.Properties.RunReport(requestBody, $"{propertyId}");
				var analyticsResponse = await analyticsRequest.ExecuteAsync();

				if (analyticsResponse.Rows != null)
				{
					foreach (var row in analyticsResponse.Rows)
					{
						if (int.TryParse(row.MetricValues.ElementAtOrDefault(0)?.Value, out var activeUsers))
						{
							totalActiveUsers += activeUsers;
						}
					}
				}
			}

			return totalActiveUsers;
		}

		public async Task<List<DashboardDimensionResponse>> GetAnalyticsDimensionData(string accessToken, string propertyId, string dimension, string metric, string startDate, string endDate)
		{
			var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);

			var service = new Google.Apis.AnalyticsData.v1beta.AnalyticsDataService(new Google.Apis.Services.BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "AnalyticsDataSample"
			});

			var requestBody = new RunReportRequest
			{
				Dimensions = new List<Dimension> { new Dimension { Name = dimension } },
				Metrics = new List<Metric> { new Metric { Name = metric } },
				DateRanges = new List<DateRange>
				{
					new DateRange
					{
						StartDate = startDate,
						EndDate = endDate
					}
				}
			};

			var request = service.Properties.RunReport(requestBody, $"properties/{propertyId}");
			var response = await request.ExecuteAsync();

			var result = new List<DashboardDimensionResponse>();

			if (response.Rows != null)
			{
				foreach (var row in response.Rows)
				{
					var metricValue = row.MetricValues.FirstOrDefault()?.Value;
					double.TryParse(metricValue, out double parsedMetric);

					result.Add(new DashboardDimensionResponse
					{
						Dimension = row.DimensionValues.FirstOrDefault()?.Value,
						Metric = parsedMetric
					});
				}
			}

			return result;
		}

		public async Task<List<GeneralCountResponse>> GetAnalyticsGeneralCount(string accessToken, string propertyId, string dimension, string startDate, string endDate)
		{
			var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);

			var service = new Google.Apis.AnalyticsData.v1beta.AnalyticsDataService(new Google.Apis.Services.BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "AnalyticsDataSample"
			});

			var requestBody = new RunReportRequest
			{
				Dimensions = new List<Dimension> { new Dimension { Name = dimension } },
				Metrics = new List<Metric>
				{
					new Metric { Name = "totalUsers" },
					new Metric { Name = "activeUsers" },
					new Metric { Name = "newUsers" },
					new Metric { Name = "screenPageViews" },
					new Metric { Name = "sessions" },
					new Metric { Name = "eventCount" },
					new Metric { Name = "keyEvents" },
					new Metric { Name = "totalRevenue" },
					new Metric { Name = "transactions" }
				},
				DateRanges = new List<DateRange>
				{
					new DateRange
					{
						StartDate = startDate,
						EndDate = endDate
					}
				}
			};

			var request = service.Properties.RunReport(requestBody, $"properties/{propertyId}");
			var response = await request.ExecuteAsync();

			var result = new List<GeneralCountResponse>();

			if (response.Rows != null)
			{
				foreach (var row in response.Rows)
				{
					result.Add(new GeneralCountResponse
					{
						Dimension = row.DimensionValues.FirstOrDefault()?.Value,
						TotalUsers = int.TryParse(row.MetricValues.ElementAtOrDefault(0)?.Value, out var totalUsers) ? totalUsers : 0,
						ActiveUsers = int.TryParse(row.MetricValues.ElementAtOrDefault(1)?.Value, out var activeUsers) ? activeUsers : 0,
						NewUsers = int.TryParse(row.MetricValues.ElementAtOrDefault(2)?.Value, out var newUsers) ? newUsers : 0,
						ScreenPageViews = int.TryParse(row.MetricValues.ElementAtOrDefault(3)?.Value, out var screenPageViews) ? screenPageViews : 0,
						Sessions = int.TryParse(row.MetricValues.ElementAtOrDefault(4)?.Value, out var sessions) ? sessions : 0,
						EventCount = int.TryParse(row.MetricValues.ElementAtOrDefault(5)?.Value, out var eventCount) ? eventCount : 0,
						KeyEvents = int.TryParse(row.MetricValues.ElementAtOrDefault(6)?.Value, out var keyEvents) ? keyEvents : 0,
						TotalRevenue = double.TryParse(row.MetricValues.ElementAtOrDefault(7)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var totalRevenue)
							? Math.Round(totalRevenue, 1) : 0,
						Transactions = int.TryParse(row.MetricValues.ElementAtOrDefault(8)?.Value, out var transactions) ? transactions : 0
					});
				}
			}

			return result;
		}

		public async Task<List<GeneralRateResponse>> GetAnalyticsGeneralRate(string accessToken, string propertyId, string dimension, string startDate, string endDate)
		{
			var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);

			var service = new Google.Apis.AnalyticsData.v1beta.AnalyticsDataService(new Google.Apis.Services.BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "AnalyticsDataSample"
			});

			var requestBody = new RunReportRequest
			{
				Dimensions = new List<Dimension> { new Dimension { Name = dimension } },
				Metrics = new List<Metric>
				{
					new Metric { Name = "averageSessionDuration" },
					new Metric { Name = "eventsPerSession" },
					new Metric { Name = "sessionKeyEventRate" },
					new Metric { Name = "screenPageViewsPerSession" },
					new Metric { Name = "engagementRate" },
					new Metric { Name = "engagedSessions" },
					new Metric { Name = "screenPageViewsPerUser" },
					new Metric { Name = "eventCountPerUser" },
					new Metric { Name = "userKeyEventRate" }
				},
				DateRanges = new List<DateRange>
				{
					new DateRange
					{
						StartDate = startDate,
						EndDate = endDate
					}
				}
			};

			var request = service.Properties.RunReport(requestBody, $"properties/{propertyId}");
			var response = await request.ExecuteAsync();

			var result = new List<GeneralRateResponse>();

			if (response.Rows != null)
			{
				foreach (var row in response.Rows)
				{
					result.Add(new GeneralRateResponse
					{
						Dimension = row.DimensionValues.FirstOrDefault()?.Value,
						AverageSessionDuration = double.TryParse(row.MetricValues.ElementAtOrDefault(0)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var averageSessionDuration)
							? Math.Round(averageSessionDuration, 1) : 0,
						EventsPerSession = double.TryParse(row.MetricValues.ElementAtOrDefault(1)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var eventsPerSession)
							? Math.Round(eventsPerSession, 1) : 0,
						SessionKeyEventRate = double.TryParse(row.MetricValues.ElementAtOrDefault(2)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sessionKeyEventRate)
							? Math.Round(sessionKeyEventRate, 1) : 0,
						ScreenPageViewsPerSession = double.TryParse(row.MetricValues.ElementAtOrDefault(3)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var screenPageViewsPerSession)
							? Math.Round(screenPageViewsPerSession, 1) : 0,
						EngagementRate = double.TryParse(row.MetricValues.ElementAtOrDefault(4)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var engagementRate)
							? Math.Round(engagementRate, 1) : 0,
						EngagedSessions = int.TryParse(row.MetricValues.ElementAtOrDefault(5)?.Value, out var engagedSessions) ? engagedSessions : 0,
						ScreenPageViewsPerUser = double.TryParse(row.MetricValues.ElementAtOrDefault(6)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var screenPageViewsPerUser)
							? Math.Round(screenPageViewsPerUser, 1) : 0,
						EventCountPerUser = double.TryParse(row.MetricValues.ElementAtOrDefault(7)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var eventCountPerUser)
							? Math.Round(eventCountPerUser, 1) : 0,
						UserKeyEventRate = double.TryParse(row.MetricValues.ElementAtOrDefault(8)?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var userKeyEventRate)
							? Math.Round(userKeyEventRate, 1) : 0
					});
				}
			}

			return result;
		}
	}
}
