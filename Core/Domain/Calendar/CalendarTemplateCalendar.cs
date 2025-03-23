using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Domain.Calendar
{
	public class CalendarTemplateCalendar : BaseEntity
	{
		public int CalendarTemplateId { get; set; }
		public virtual CalendarTemplate CalendarTemplate { get; set; }
		public int CalendarId { get; set; }
		public virtual Calendar Calendar { get; set; }
	}
}
