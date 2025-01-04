using Newtonsoft.Json;

namespace AdminPanel.Models.Meta.Charts
{
    public class ApiResponse
    {
        [JsonProperty("2024-01")]
        public MonthData Ocak { get; set; }

        [JsonProperty("2024-02")]
        public MonthData Şubat { get; set; }

        [JsonProperty("2024-03")]
        public MonthData Mart { get; set; }

        [JsonProperty("2024-04")]
        public MonthData Nisan { get; set; }

        [JsonProperty("2024-05")]
        public MonthData Mayıs { get; set; }

        [JsonProperty("2024-06")]
        public MonthData Haziran { get; set; }

        [JsonProperty("2024-07")]
        public MonthData Temmuz { get; set; }

        [JsonProperty("2024-08")]
        public MonthData Ağustos { get; set; }

        [JsonProperty("2024-09")]
        public MonthData Eylül { get; set; }

        [JsonProperty("2024-10")]
        public MonthData Ekim { get; set; }

        [JsonProperty("2024-11")]
        public MonthData Kasım { get; set; }

        [JsonProperty("2024-12")]
        public MonthData Aralık { get; set; }
    }
}
