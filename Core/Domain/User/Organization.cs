using Core.Domain.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; }
        public int UserCount { get; set; }
        public int AccountCount { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string TaskNumber { get; set; }
        public string Phone { get; set; }
        public string AccountType { get; set; }
        public string? MetaAccount { get; set; }
        public string? GoogleAccount { get; set; }
        public string? GoogleAnalytics { get; set; }
        public string? GoogleSearchConsole { get; set; }
        public virtual ICollection<User> User { get; set; }
        public virtual ICollection<Report.Report> Report { get; set; }
        public virtual ICollection<Core.Domain.Task.Task> Task { get; set; }
        public virtual ICollection<Core.Domain.Task.TaskTemplate> TaskTemplate { get; set; }
		public virtual ICollection<Calendar.Calendar> Calendar { get; set; }
	}
}
