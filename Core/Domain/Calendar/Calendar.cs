using Core.Domain.Task;
using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Calendar
{
	public class Calendar : BaseEntity
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Color { get; set; }
		public string? Mail { get; set; }
		public string? Phone { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
        public bool IsConfirmation { get; set; }
        public bool AllDay { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int OrganizationId { get; set; }
		public virtual Organization Organization { get; set; }
		public virtual ICollection<CalendarTemplateCalendar> CalendarTemplateCalendar { get; set; }
	}
}
