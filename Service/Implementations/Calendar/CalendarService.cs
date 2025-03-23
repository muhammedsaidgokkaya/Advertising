using Core.Data;
using Core.Domain.Calendar;
using Core.Domain.Task;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Service.Implementations.Task;
using Service.Interfaces.Calendar;
using System.Numerics;

namespace Service.Implementations.Calendar
{
	public class CalendarService : ICalendarService
	{
		private readonly Repository<Context> _repository;

		public CalendarService()
		{
			_repository = new Repository<Context>(new Context());
		}

		public int AddCalendarTemplate(int organizationId, string name)
		{
			var calendarTemplate = new CalendarTemplate
			{
				KeyName = name,
				OrganizationId = organizationId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			_repository.Save(calendarTemplate);
			return calendarTemplate.Id;
		}

		public int AddCalendar(int organizationId, string title, string description, string color, bool allDay, DateTime start, DateTime end, string mail, string phone, string firstName, string lastName, bool isConfirmation)
		{
			var calendar = new Core.Domain.Calendar.Calendar
			{
				Title = title,
				Description = description,
				Color = color,
				Mail = mail,
				Phone = phone,
				FirstName = firstName,
				LastName = lastName,
				AllDay = allDay,
				IsConfirmation = isConfirmation,
				Start = start.ToUniversalTime(),
				End = end.ToUniversalTime(),
				OrganizationId = organizationId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			_repository.Save(calendar);
			return calendar.Id;
		}

		public int UpdateCalendar(int id, string title, string description, string color, bool allDay, DateTime start, DateTime end, string mail, string phone, string firstName, string lastName, bool isConfirmation)
		{
			var calendar = GetCalendarById(id);
			if (calendar != null)
			{
				calendar.Title = title;
				calendar.Description = description;
				calendar.Color = color;
				calendar.Mail = mail;
				calendar.Phone = phone;
				calendar.FirstName = firstName;
				calendar.LastName = lastName;
				calendar.IsConfirmation = isConfirmation;
				calendar.AllDay = allDay;
				calendar.Start = start.ToUniversalTime();
				calendar.End = end.ToUniversalTime();
				calendar.UpdateDate = DateTime.UtcNow;

				_repository.Update(calendar);
				return calendar.Id;
			}
			return 0;
		}

		public int IsDeletedCalendarTemplate(int id)
		{
			var calendarTemplate = GetCalendarTemplateById(id);
			if (calendarTemplate != null)
			{
				calendarTemplate.IsDeleted = !calendarTemplate.IsDeleted;
				calendarTemplate.UpdateDate = DateTime.UtcNow;

				_repository.Update(calendarTemplate);
				return calendarTemplate.Id;
			}
			return 0;
		}

		public int IsDeletedCalendar(int id)
		{
			var calendar = GetCalendarById(id);
			if (calendar != null)
			{
				calendar.IsDeleted = !calendar.IsDeleted;
				calendar.UpdateDate = DateTime.UtcNow;

				_repository.Update(calendar);
				return calendar.Id;
			}
			return 0;
		}

		public Core.Domain.Calendar.Calendar GetCalendarById(int id)
		{
			return _repository.GetById<Core.Domain.Calendar.Calendar>(id);
		}

		public Core.Domain.Calendar.CalendarTemplate GetCalendarTemplateById(int id)
		{
			return _repository.GetById<Core.Domain.Calendar.CalendarTemplate>(id);
		}

		public async Task<IEnumerable<CalendarTemplate>> GetCalendarTemplate(int organizationId)
		{
			var data = _repository.FilterAsQueryable<CalendarTemplate>(p => !p.IsDeleted && p.Organization.Id.Equals(organizationId)).IncludeCalendarTemplate();
			return data;
		}

		public async Task<IEnumerable<Core.Domain.Calendar.Calendar>> GetCalenders(int organizationId)
		{
			var data = _repository.FilterAsQueryable<Core.Domain.Calendar.Calendar>(p => !p.IsDeleted && p.Organization.Id.Equals(organizationId)).IncludeCalendar();
			return data;
		}

		public async Task<IEnumerable<Core.Domain.Calendar.Calendar>> GetCalendersHashCode(string organization)
		{
			var data = _repository.FilterAsQueryable<Core.Domain.Calendar.Calendar>(p => !p.IsDeleted && p.Organization.OrganizationHashCode.Equals(organization)).IncludeCalendar();
			return data;
		}
	}

	public static class CalendarExtensions
	{
		public static IQueryable<Core.Domain.Calendar.Calendar> IncludeCalendar(this IQueryable<Core.Domain.Calendar.Calendar> query)
		{
			return query
				.Include(ma => ma.Organization);
		}

		public static IQueryable<CalendarTemplate> IncludeCalendarTemplate(this IQueryable<CalendarTemplate> query)
		{
			return query
				.Include(ma => ma.Organization);
		}

		public static IQueryable<CalendarTemplateCalendar> IncludeCalendarTemplateTask(this IQueryable<CalendarTemplateCalendar> query)
		{
			return query
				.Include(ma => ma.Calendar)
				.Include(ma => ma.CalendarTemplate);
		}
	}
}
