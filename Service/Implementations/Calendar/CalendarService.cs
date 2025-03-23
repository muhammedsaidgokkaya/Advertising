using Core.Data;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Service.Implementations.Task;
using Service.Interfaces.Calendar;

namespace Service.Implementations.Calendar
{
	public class CalendarService : ICalendarService
	{
		private readonly Repository<Context> _repository;

		public CalendarService()
		{
			_repository = new Repository<Context>(new Context());
		}

		public int AddCalendar(int organizationId, string title, string description, string color, bool allDay, DateTime start, DateTime end)
		{
			var calendar = new Core.Domain.Calendar.Calendar
			{
				Title = title,
				Description = description,
				Color = color,
				AllDay = allDay,
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

		public int UpdateCalendar(int id, string title, string description, string color, bool allDay, DateTime start, DateTime end)
		{
			var calendar = GetCalendarById(id);
			if (calendar != null)
			{
				calendar.Title = title;
				calendar.Description = description;
				calendar.Color = color;
				calendar.AllDay = allDay;
				calendar.Start = start.ToUniversalTime();
				calendar.End = end.ToUniversalTime();
				calendar.UpdateDate = DateTime.UtcNow;

				_repository.Update(calendar);
				return calendar.Id;
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
	}
}
