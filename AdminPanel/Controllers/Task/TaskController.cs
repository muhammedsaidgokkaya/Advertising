using AdminPanel.Controllers.Organization;
using AdminPanel.Models.Organization.User;
using AdminPanel.Models.Task.Task;
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

		[HttpGet("schemas")]
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

		[HttpGet("tasks")]
		public async Task<ActionResult<IEnumerable<Models.Task.Task.Tasks>>> GetTasks()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var tasks = await _taskService.GetTask(user.OrganizationId);

			var taskList = tasks.Select(task =>
			{
				var createdUser = _userService.GetUserById(task.CreatedUser);
				return new Models.Task.Task.Tasks
				{
					Id = task.Id,
					CreatedDate = task.InsertedDate ?? DateTime.MinValue,
					Name = task.TaskName,
					State = task.State,
					CreatedUser = createdUser != null ? $"{createdUser.FirstName} {createdUser.LastName}" : "Bilinmiyor",
					Duration = task.Deadline ?? DateTime.MinValue,
					Team = task.TaskUser.Count,
				};
			}).ToList();

			return Ok(taskList);
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

		[HttpPost]
		[Route("add-task")]
		public async Task<IActionResult> AddTask([FromBody] AddTask request)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			string departman = string.Join(", ", request.Departments);

			var addTask = _taskService.AddTask(userId, user.OrganizationId, request.Name, request.Content, request.Durations.ToUniversalTime(), departman);
            
			if (addTask == 0)
            {
				return Ok(0);
            }

            if (request.Users != null || request.Users.Count != 0 )
            {
				foreach (var item in request.Users)
				{
					_taskService.AddTaskUser(item, addTask);

				}
			}

            if (request.Services != null || request.Services.Count != 0 )
            {
				foreach (var item in request.Services)
				{
					_taskService.AddTaskTemplateTask(item, addTask);

				}
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
