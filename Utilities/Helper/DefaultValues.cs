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
        public List<DateTime> DefaultDate(DateTime? startDate = null, DateTime? endDate = null, int timeNumber = 30)
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

        public string GoogleProperty(string property)
        {
            var propertyId = property.Split('/').Last().TrimEnd('\"');
            return propertyId;
        }
    }
}
