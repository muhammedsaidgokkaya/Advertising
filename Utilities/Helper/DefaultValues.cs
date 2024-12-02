using System;
using System.Collections.Generic;
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

        public string GoogleProperty(string property)
        {
            var propertyId = property.Split('/').Last().TrimEnd('\"');
            return propertyId;
        }
    }
}
