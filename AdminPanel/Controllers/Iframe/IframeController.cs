using AdminPanel.Controllers.Calendar;
using AdminPanel.Models.Calendar;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Calendar;
using Service.Implementations.User;
using Utilities.Helper;

namespace AdminPanel.Controllers.Iframe
{
	[Route("api/[controller]")]
	[ApiController]
	public class IframeController : ControllerBase
	{
		private readonly ILogger<IframeController> _logger;
		private readonly UserService _userService;
		private readonly CalendarService _calendarService;
		private readonly DefaultValues _defaultValues;
		private readonly EmailHelper _emailHelper;

		public IframeController(ILogger<IframeController> logger)
		{
			_logger = logger;
			_userService = new UserService();
			_calendarService = new CalendarService();
			_defaultValues = new DefaultValues();
			_emailHelper = new EmailHelper();
		}

		[HttpGet("calendars")]
		public async Task<ActionResult<IEnumerable<Models.Calendar.GetCalendars>>> GetCalendars(string organization)
		{
			var calendars = await _calendarService.GetCalendersHashCode(organization);

			var calendarsList = calendars.Select(calendar => new Models.Calendar.GetCalendars
			{
				Id = calendar.Id,
				Title = calendar.AllDay ? "Tüm Gün" : "",
				Description = calendar.Description,
				Color = calendar.Color,
				AllDay = calendar.AllDay,
				Start = calendar.Start,
				End = calendar.End,
			}).ToList();

			return Ok(calendarsList);
		}

		[HttpPost]
		[Route("add-calendar")]
		public async Task<IActionResult> AddOrUpdateCalendar([FromBody] AddOrUpdateCalendar request, string organization)
		{
			var org = _userService.GetOrganizationHashCode(organization);
			if (request.Id == 0)
			{
				var addCalendar = _calendarService.AddCalendar(org.Id, request.Title, request.Description, request.Color, request.AllDay, request.Start, request.End);
			}
			return Ok(1);
		}
	}
}
