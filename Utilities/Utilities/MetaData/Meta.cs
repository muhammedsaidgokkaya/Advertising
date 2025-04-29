using NPOI.XSSF.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Utilities.Utilities.MetaData
{
	public class Meta
	{
		private readonly HttpClient _httpClient;

		public Meta(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<List<AdInfo>> GetAllAdsAsync(string accessToken, List<string> adAccounts)
		{
			var allAds = new List<AdInfo>();

			foreach (var account in adAccounts)
			{
				var ads = await GetAdsFromAccountAsync(accessToken, account);

				if (ads.ContainsKey("error"))
				{
					allAds.Add(new AdInfo { Error = ads["error"].ToString() });
				}
				else
				{
					foreach (var ad in ads["data"].EnumerateArray())
					{
						var name = ad.GetProperty("name").GetString();
						var imageUrl = ad.TryGetProperty("creative", out var creative) &&
									   creative.TryGetProperty("image_url", out var image)
									   ? image.GetString()
									   : null;

						allAds.Add(new AdInfo
						{
							Name = name,
							ImageUrl = imageUrl
						});
					}
				}
			}

			return allAds;
		}

		private async Task<Dictionary<string, JsonElement>> GetAdsFromAccountAsync(string accessToken, string adAccountId)
		{
			var url = $"https://graph.facebook.com/v21.0/{adAccountId}/ads?access_token={accessToken}&fields=id,name,creative{{image_url}}&limit=3";
			var response = await _httpClient.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();
			var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

			return json;
		}

		public async Task<Dictionary<string, object>> FetchMetricAsync(string accessToken, string adAccountId, string metric)
		{
			var url = $"https://graph.facebook.com/v21.0/{adAccountId}/insights" +
					  $"?access_token={accessToken}&level=account&fields={metric}&date_preset=maximum";

			try
			{
				var response = await _httpClient.GetAsync(url);
				var content = await response.Content.ReadAsStringAsync();
				var json = JsonDocument.Parse(content);

				if (response.IsSuccessStatusCode)
				{
					double total = 0;
					var data = json.RootElement.GetProperty("data");

					foreach (var row in data.EnumerateArray())
					{
						if (row.TryGetProperty(metric, out var metricValue))
						{
							if (double.TryParse(metricValue.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
							{
								total += value;
							}
						}
					}

					return new Dictionary<string, object> { [adAccountId] = total };
				}
				else
				{
					return new Dictionary<string, object> { [adAccountId] = new { error = json.RootElement } };
				}
			}
			catch (Exception ex)
			{
				return new Dictionary<string, object> { [adAccountId] = new { error = ex.Message } };
			}
		}

		public async Task<MetricResult> FetchAllMetricsAsync(string accessToken, string metric, List<string> adAccounts)
		{
			var tasks = adAccounts.Select(id => FetchMetricAsync(accessToken, id, metric)).ToList();

			var responses = await Task.WhenAll(tasks);
			var result = new MetricResult();

			foreach (var response in responses)
			{
				foreach (var entry in response)
				{
					result.Results[entry.Key] = entry.Value;

					if (entry.Value is double val)
					{
						result.Total += val;
					}
				}
			}

			return result;
		}

		public async Task<CombinedMetrics> GetCombinedMetricsAsync(string accessToken, List<string> adAccounts)
		{
			var spendTask = FetchAllMetricsAsync(accessToken, "spend", adAccounts);
			var impressionsTask = FetchAllMetricsAsync(accessToken, "impressions", adAccounts);
			var clicksTask = FetchAllMetricsAsync(accessToken, "clicks", adAccounts);

			await Task.WhenAll(spendTask, impressionsTask, clicksTask);

			return new CombinedMetrics
			{
				Spend = spendTask.Result.Total,
				Impressions = impressionsTask.Result.Total,
				Clicks = clicksTask.Result.Total
			};
		}
	}

	public class AdInfo
	{
		public string Name { get; set; }
		public string ImageUrl { get; set; }
		public string Error { get; set; }
	}

	public class MetricResult
	{
		public Dictionary<string, object> Results { get; set; } = new();
		public double Total { get; set; }
	}

	public class CombinedMetrics
	{
		public double Spend { get; set; }
		public double Impressions { get; set; }
		public double Clicks { get; set; }
	}
}
