using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Report
{
    public class Report : BaseEntity
    {
        public string Name { get; set; }
        public string Account { get; set; }
        public string AccountId { get; set; }
        public string TypeId { get; set; }
        public string Content { get; set; }
        public int ReportType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
