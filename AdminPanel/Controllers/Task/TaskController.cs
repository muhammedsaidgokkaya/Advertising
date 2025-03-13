using AdminPanel.Controllers.Organization;
using AdminPanel.Models.Organization.User;
using AdminPanel.Models.Task.TaskTemplate;
using Core.Domain.Task;
using Core.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Task;
using Service.Implementations.User;
using System.Text.Json.Serialization;
using Utilities.Helper;

namespace AdminPanel.Controllers.Task
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class TaskController : ControllerBase
	{
		private readonly ILogger<TaskController> _logger;
		private readonly UserService _userService;
		private readonly TaskService _taskService;
		private readonly DefaultValues _defaultValues;
		private readonly EmailHelper _emailHelper;

		public TaskController(ILogger<TaskController> logger)
		{
			_logger = logger;
			_userService = new UserService();
			_taskService = new TaskService();
			_defaultValues = new DefaultValues();
			_emailHelper = new EmailHelper();
		}

		[HttpGet("schema")]
		public async Task<ActionResult<IEnumerable<Models.Task.TaskTemplate.TaskTemplate>>> GetSchemas()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var schemas = await _taskService.GetTaskTemplate(user.OrganizationId);

			var schemasList = schemas.Select(schema => new Models.Task.TaskTemplate.TaskTemplate
			{
				Id = schema.Id,
				Name = schema.KeyName
			}).ToList();

			return Ok(schemasList);
		}

		[HttpPost]
		[Route("add-schema")]
		public async Task<IActionResult> AddSchema([FromBody] AddSchema request)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var existingTemplates = await _taskService.GetTaskTemplate(user.OrganizationId);

			var existingTemplateNames = existingTemplates.Select(t => t.KeyName).ToList();

			var templatesToAdd = request.Name.Except(existingTemplateNames).ToList(); 
			var templatesToRemoveNames = existingTemplateNames.Except(request.Name).ToList();
			var templatesToRemove = existingTemplates
				.Where(t => templatesToRemoveNames.Contains(t.KeyName))
				.Select(t => t.Id)
				.ToList();

			foreach (var template in templatesToAdd)
			{
				_taskService.AddTaskTemplate(user.OrganizationId, template);
			}

			foreach (var template in templatesToRemove)
			{
				_taskService.IsDeletedTaskTemplate(template);
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
