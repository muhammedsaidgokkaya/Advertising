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

        public string GoogleProperty(string property)
        {
            var propertyId = property.Split('/').Last().TrimEnd('\"');
            return propertyId;
        }
    }
}
