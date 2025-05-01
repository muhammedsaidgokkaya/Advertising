using Newtonsoft.Json;
using NPOI.XSSF.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Utilities.Utilities.MetaData.MetaData;

namespace Utilities.Utilities.MetaData
{
	public class Meta
	{
		private readonly HttpClient _httpClient;

		public Meta(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<BusinessResponse> GetFacebookBusinesses(string accessToken)
		{
			string apiVersion = "v21.0";
			string url = $"https://graph.facebook.com/{apiVersion}/me/businesses?access_token={accessToken}";

			using var client = new HttpClient();
			try
			{
				var response = await client.GetAsync(url);
				var content = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					return JsonConvert.DeserializeObject<BusinessResponse>(content);
				}
				else
				{
					Console.WriteLine($"API Hatası: {content}");
					return null;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"İstek hatası: {ex.Message}");
				return null;
			}
		}

		public async Task<AdvertisingAccountsResponse> GetFacebookAdAccounts(string accessToken, string businessId)
		{
			string apiVersion = "v21.0";
			string url = $"https://graph.facebook.com/{apiVersion}/{businessId}/owned_ad_accounts?fields=id,name&access_token={accessToken}";

			using var client = new HttpClient();
			try
			{
				var response = await client.GetAsync(url);
				var content = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode)
				{
					return JsonConvert.DeserializeObject<AdvertisingAccountsResponse>(content);
				}
				else
				{
					Console.WriteLine($"API Hatası: {content}");
					return null;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"İstek hatası: {ex.Message}");
				return null;
			}
		}

		public async Task<List<AdInfo>> GetAllAdsAsync(string accessToken, List<string> adAccounts)
		{
			var allAds = new List<AdInfo>();

			foreach (var account in adAccounts)
			{
				var ads = await GetAdsFromAccountAsync(accessToken, account);

				foreach (var ad in ads.Data)
				{
					var name = ad.Name;
					var imageUrl = ad.Creative?.ImageUrl;

					allAds.Add(new AdInfo
					{
						Name = name,
						ImageUrl = imageUrl
					});
				}
			}
			return allAds;
		}

		public async Task<AdData> GetAdsFromAccountAsync(string accessToken, string adAccountId)
		{
			var url = $"https://graph.facebook.com/v21.0/{adAccountId}/ads?access_token={accessToken}&fields=id,name,creative{{image_url}}&limit=3";
			var response = await _httpClient.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();
			var jsons = Newtonsoft.Json.JsonConvert.DeserializeObject<AdData>(content);

			return jsons;
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

	public class AdData
	{
		[JsonProperty("data")]
		public List<AdRes> Data { get; set; }

		[JsonProperty("paging")]
		public Paging Paging { get; set; }
	}

	public class AdRes
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("creative")]
		public Creative Creative { get; set; }
	}

	public class Creative
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("image_url")]
		public string ImageUrl { get; set; }
	}

	public class Paging
	{
		[JsonProperty("cursors")]
		public Cursors Cursors { get; set; }
	}

	public class Cursors
	{
		[JsonProperty("before")]
		public string Before { get; set; }

		[JsonProperty("after")]
		public string After { get; set; }
	}
}
