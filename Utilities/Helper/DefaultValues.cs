using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Helper
{
    public class DefaultValues
    {
        public List<DateTime> DefaultDate(DateTime? startDate = null, DateTime? endDate = null, int timeNumber = 90)
        {
            DateTime defaultEndDate = endDate ?? DateTime.Now;
            DateTime defaultStartDate = startDate ?? defaultEndDate.AddDays(-timeNumber);
            if (startDate.HasValue && !endDate.HasValue)
            {
                defaultEndDate = startDate.Value.AddDays(1);
            }
            else if (!startDate.HasValue && endDate.HasValue)
            {
                defaultStartDate = endDate.Value.AddDays(-1);
            }
            List<DateTime> result = new List<DateTime>();
            result.Add(defaultStartDate);
            result.Add(defaultEndDate);
            return result;
        }

        public List<DateTime> DefaultMounth(DateTime? startDate = null, DateTime? endDate = null, int timeNumber = 6)
        {
            DateTime defaultEndDate = endDate ?? DateTime.Now;
            DateTime defaultStartDate = startDate ?? defaultEndDate.AddMonths(-timeNumber);
            if (startDate.HasValue && !endDate.HasValue)
            {
                defaultEndDate = startDate.Value.AddDays(1);
            }
            else if (!startDate.HasValue && endDate.HasValue)
            {
                defaultStartDate = endDate.Value.AddDays(-1);
            }
            List<DateTime> result = new List<DateTime>();
            result.Add(defaultStartDate);
            result.Add(defaultEndDate);
            return result;
        }

        public string RemoveDiacritics(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            str = str.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in str)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public string GenerateRandomPassword(int length = 9)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var password = new char[length];
            Random _random = new Random();

            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[_random.Next(validChars.Length)];
            }

            return new string(password);
        }

        public string HashPassword(string password)
        {
            using (var hasher = new System.Security.Cryptography.SHA256Managed())
            {
                var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hashedBytes = hasher.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public string GetFormattedBidStrategy(string bidStrategy)
        {
            switch (bidStrategy)
            {
                case "LOWEST_COST_WITHOUT_CAP":
                    return "En Düşük Maliyet (Limit Yok)";
                case "LOWEST_COST_WITH_BID_CAP":
                    return "En Düşük Maliyet (Teklif Limiti)";
                case "COST_CAP":
                    return "Maliyet Limiti";
                case "LOWEST_COST_WITH_MIN_ROAS":
                    return "En Düşük Maliyet (Minimum ROAS)";
                default:
                    return bidStrategy;
            }
        }

        public string FormatRanking(string ranking)
        {
            if (string.IsNullOrEmpty(ranking))
                return "—";

            var parts = ranking.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out int numericValue))
            {
                switch (parts[0])
                {
                    case "ABOVE_AVERAGE":
                        return $"Ortalama Üzerinde (%{numericValue})";
                    case "AVERAGE":
                        return $"Ortalama (%{numericValue})";
                    case "BELOW_AVERAGE":
                        return $"Ortalamanın Altında (%{numericValue})";
                    case "UNKNOWN":
                        return "—";
                    default:
                        return "—";
                }
            }

            switch (ranking)
            {
                case "ABOVE_AVERAGE":
                    return "Üstün Ortalama";
                case "AVERAGE":
                    return "Ortalama";
                case "BELOW_AVERAGE":
                    return "Alt Ortalamanın Altında";
                case "UNKNOWN":
                    return "—";
                default:
                    return "—";
            }
        }

        public string ProcessResults(string objective, IEnumerable<Action> actions)
        {
            if (actions == null || !actions.Any())
                return "Sonuç bulunamadı.";

            var result = objective switch
            {
                "OUTCOME_ENGAGEMENT" => GenerateResultString(actions, new Dictionary<string, string>
                {
                    { "onsite_conversion.messaging_conversation_started_7d", "Mesajlaşma konuşması başlatma" }
                }),
                    "OUTCOME_TRAFFIC" => GenerateResultString(actions, new Dictionary<string, string>
                {
                    { "page_engagement", "Sayfa Etkileşimleri" }
                }),
                    "OUTCOME_LEADS" => GenerateResultString(actions, new Dictionary<string, string>
                {
                    { "lead", "Oluşturulan Leadler" }
                }),
                    "OUTCOME_APP_PROMOTION" => GenerateResultString(actions, new Dictionary<string, string>
                {
                    { "app_install", "Uygulama Kurulumları" }
                }),
                    "OUTCOME_AWARENESS" => GenerateResultString(actions, new Dictionary<string, string>
                {
                    { "post_reaction", "Gönderi Reaksiyonları" }
                }),
                    "OUTCOME_SALES" => GenerateResultString(actions, new Dictionary<string, string>
                {
                    { "purchase", "Satın Alımlar" }
                }),
                _ => "Hedef için sonuçlar tanımlı değil."
            };

            return result;
        }

        private string GenerateResultString(IEnumerable<Action> actions, Dictionary<string, string> actionDescriptions)
        {
            var resultStrings = actionDescriptions.Select(desc =>
            {
                var count = actions.FirstOrDefault(a => a.ActionType == desc.Key)?.Value ?? 0;
                return $"{desc.Value}";
            });

            return string.Join(", ", resultStrings);
        }

        public double ProcessResultsInt(string objective, IEnumerable<Action> actions)
        {
            if (actions == null || !actions.Any())
                return 0;

            var result = objective switch
            {
                "OUTCOME_ENGAGEMENT" => GenerateResultInt(actions, new Dictionary<string, string>
                {
                    { "onsite_conversion.messaging_conversation_started_7d", "Mesajlaşma konuşması başlatma" }
                }),
                        "OUTCOME_TRAFFIC" => GenerateResultInt(actions, new Dictionary<string, string>
                {
                    { "page_engagement", "Sayfa Etkileşimleri" }
                }),
                        "OUTCOME_LEADS" => GenerateResultInt(actions, new Dictionary<string, string>
                {
                    { "lead", "Oluşturulan Leadler" }
                }),
                        "OUTCOME_APP_PROMOTION" => GenerateResultInt(actions, new Dictionary<string, string>
                {
                    { "app_install", "Uygulama Kurulumları" }
                }),
                        "OUTCOME_AWARENESS" => GenerateResultInt(actions, new Dictionary<string, string>
                {
                    { "post_reaction", "Gönderi Reaksiyonları" }
                }),
                        "OUTCOME_SALES" => GenerateResultInt(actions, new Dictionary<string, string>
                {
                    { "purchase", "Satın Alımlar" }
                }),
                _ => 0
            };

            return result;
        }

        private double GenerateResultInt(IEnumerable<Action> actions, Dictionary<string, string> actionDescriptions)
        {
            return actionDescriptions
                .Select(desc => actions.FirstOrDefault(a => a.ActionType == desc.Key)?.Value ?? 0)
                .Sum();
        }

        public class Action
        {
            public string ActionType { get; set; }

            public double Value { get; set; }
        }

        public string GetGenderString(int genderValue)
        {
            return genderValue switch
            {
                0 => "Tümü",
                1 => "Erkek",
                2 => "Kadın",
                _ => "—"
            };
        }

        public string GetAgeRangeString(int? ageFrom, int? ageTo)
        {
            if (ageFrom.HasValue && ageTo.HasValue)
            {
                return $"{ageFrom.Value} - {ageTo.Value}";
            }
            return "—";
        }

        public string GetCountryNameFormat(string country, string cityName)
        {
            return string.IsNullOrEmpty(country) || string.IsNullOrEmpty(cityName) ? "—" : $"{country}/{cityName}";
        }

        public string GetTargetAudienceSize(int upperBound, int lowerBound)
        {
            string upperBoundFormatted = upperBound.ToString("N0");
            string lowerBoundFormatted = lowerBound.ToString("N0");

            if (upperBound == lowerBound)
            {
                return $"{upperBoundFormatted}'den az";
            }
            else
            {
                return $"{Math.Min(upperBound, lowerBound):N0} - {Math.Max(upperBound, lowerBound):N0}";
            }
        }

        public string GetAudienceTypeText(string audienceType)
        {
            return audienceType switch
            {
                "saved" => "Kaydedilen Hedef Kitle",
                "custom" => "Özel Hedef Kitle",
                "lookalike" => "Benzer Hedef Kitle",
                _ => "—"
            };
        }

        public string GoogleProperty(string property)
        {
            var propertyId = property.Split('/').Last().TrimEnd('\"');
            return propertyId;
        }
    }
}
