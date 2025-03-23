using AdminPanel.Controllers.Task;
using AdminPanel.Models.Calendar;
using AdminPanel.Models.Task.Task;
using AdminPanel.Models.Task.TaskTemplate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Calendar;
using Service.Implementations.Task;
using Service.Implementations.User;
using Utilities.Helper;

namespace AdminPanel.Controllers.Calendar
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class CalendarController : ControllerBase
	{
		private readonly ILogger<CalendarController> _logger;
		private readonly UserService _userService;
		private readonly CalendarService _calendarService;
		private readonly DefaultValues _defaultValues;
		private readonly EmailHelper _emailHelper;

		public CalendarController(ILogger<CalendarController> logger)
		{
			_logger = logger;
			_userService = new UserService();
			_calendarService = new CalendarService();
			_defaultValues = new DefaultValues();
			_emailHelper = new EmailHelper();
		}

		[HttpGet("schemas")]
		public async Task<ActionResult<IEnumerable<Models.Calendar.CalendarTemplate>>> GetSchemas()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var schemas = await _calendarService.GetCalendarTemplate(user.OrganizationId);

			var schemasList = schemas.Select(schema => new Models.Calendar.CalendarTemplate
			{
				Id = schema.Id,
				Name = schema.KeyName
			}).ToList();

			return Ok(schemasList);
		}

		[HttpGet("calendars")]
		public async Task<ActionResult<IEnumerable<Models.Calendar.GetCalendars>>> GetCalendars()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var calendars = await _calendarService.GetCalenders(user.OrganizationId);

			var calendarsList = calendars.Select(calendar => new Models.Calendar.GetCalendars
			{
				Id = calendar.Id,
				Title = calendar.Title,
				Description = calendar.Description,
				Color = calendar.Color,
				AllDay = calendar.AllDay,
				Start = calendar.Start,
				End = calendar.End,
			}).ToList();

			return Ok(calendarsList);
		}

		[HttpPost]
		[Route("add-schema")]
		public async Task<IActionResult> AddSchema([FromBody] AddSchemaCalendar request)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var existingTemplates = await _calendarService.GetCalendarTemplate(user.OrganizationId);

			var existingTemplateNames = existingTemplates.Select(t => t.KeyName).ToList();

			var templatesToAdd = request.Name.Except(existingTemplateNames).ToList();
			var templatesToRemoveNames = existingTemplateNames.Except(request.Name).ToList();
			var templatesToRemove = existingTemplates
				.Where(t => templatesToRemoveNames.Contains(t.KeyName))
				.Select(t => t.Id)
				.ToList();

			foreach (var template in templatesToAdd)
			{
				var addTemplate = _calendarService.AddCalendarTemplate(user.OrganizationId, template);
			}

			foreach (var template in templatesToRemove)
			{
				var deleteTemplate = _calendarService.IsDeletedCalendarTemplate(template);
			}

			return Ok(1);
		}

		[HttpPost]
		[Route("add-update-calendar")]
		public async Task<IActionResult> AddOrUpdateCalendar([FromBody] AddOrUpdateCalendar request)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
            if (request.Id == 0)
            {
				var addCalendar = _calendarService.AddCalendar(user.OrganizationId, request.Title, request.Description, request.Color, request.AllDay, request.Start, request.End);
			}
            else
            {
				var updateCalendar = _calendarService.UpdateCalendar(request.Id, request.Title, request.Description, request.Color, request.AllDay, request.Start, request.End);
			}
            return Ok(1);
		}

		[HttpPost]
		[Route("delete-calendar")]
		public async Task<IActionResult> DeleteCalendar(int calendarId)
		{
			var deleteCalendar = _calendarService.IsDeletedCalendar(calendarId);
			if (deleteCalendar == 0)
			{
				return Ok(0);
			}
			return Ok(1);
		}

		private int UserId()
		{
			var userIdClaim = HttpContext.User.FindFirst("userId");
			if (userIdClaim == null)
			{
				return 0;
			}

			int userId = int.Parse(userIdClaim.Value);
			return userId;
		}
	}
}
