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
using System.Threading.Tasks;
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

		[HttpGet("task-schemas")]
		public async Task<ActionResult<IEnumerable<Models.Task.TaskTemplate.TaskSchema>>> GetTaskSchemas(int taskId)
		{
			var schemas = await _taskService.GetTaskTemplateTask(taskId);

			var schemasList = schemas.Select(schema => new Models.Task.TaskTemplate.TaskSchema
			{
				Id = schema.Id,
				Name = schema.TaskTemplate.KeyName,
				IsFinished = schema.IsFinished,
			}).ToList();

			return Ok(schemasList);
		}

		[HttpGet("task-users")]
		public async Task<ActionResult<IEnumerable<Models.Task.Task.TaskUser>>> GetTaskUsers(int taskId)
		{
			var taskUsers = await _taskService.GetTaskUser(taskId);

			var taskUserList = taskUsers.Select(taskUser => new Models.Task.Task.TaskUser
			{
				Label = taskUser.User.FirstName + " " + taskUser.User.LastName,
				Title = taskUser.User.Title,
			}).ToList();

			return Ok(taskUserList);
		}

		[HttpGet("task-comments")]
		public async Task<ActionResult<IEnumerable<Models.Task.TaskComment.TaskComments>>> GetTaskComments(int taskId)
		{
			var taskComments = await _taskService.GetTaskComment(taskId);

			var taskCommentList = taskComments.Select(taskComment => new Models.Task.TaskComment.TaskComments
			{
				Id = taskComment.Id,
				Name = taskComment.User.FirstName + " " + taskComment.User.LastName,
				PostedAt = taskComment.InsertedDate ?? DateTime.MinValue,
				Message = taskComment.Comment,
				UserId = taskComment.UserId,
			}).ToList();

			return Ok(taskCommentList);
		}

		[HttpGet("tasks")]
		public async Task<ActionResult<IEnumerable<Models.Task.Task.Tasks>>> GetTasks()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var tasks = await _taskService.GetTasks(user.OrganizationId);

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
			})
			.OrderByDescending(task => task.CreatedDate)
			.ToList();

			return Ok(taskList);
		}

		[HttpGet("task")]
		public ActionResult<GetTask> GetTask(int taskId)
		{
			var task = _taskService.GetTaskById(taskId);
			var createdUser = _userService.GetUserById(task.CreatedUser);
			var data = new GetTask
			{
				Id = task.Id,
				Name = task.TaskName,
				CreatedDate = task.InsertedDate ?? DateTime.MinValue,
				State = task.State,
				CreatedUser = createdUser != null ? $"{createdUser.FirstName} {createdUser.LastName}" : "Bilinmiyor",
				Duration = task.Deadline ?? DateTime.MinValue,
				Content = task.Description,
				Department = task.Departments,
			};

			return Ok(data);
		}

		[HttpGet("get-update-task")]
		public async Task<ActionResult<UpdateTask>> GetUpdateTask(int taskId)
		{
			var task = _taskService.GetTaskById(taskId);
			var createdUser = _userService.GetUserById(task.CreatedUser);
			var taskUsers = await _taskService.GetTaskUser(taskId);
			var taskServices = await _taskService.GetTaskTemplateTask(taskId);

			var data = new UpdateTask
			{
				Id = task.Id,
				Name = task.TaskName,
				Content = task.Description,
				Durations = task.Deadline ?? DateTime.MinValue,
				Departments = task.Departments?.Split(',').Select(d => d.Trim()).ToArray() ?? Array.Empty<string>(),
				Users = taskUsers.Select(tu => new UserDto
				{
					Id = tu.UserId,
					Name = tu.User.FirstName + " " + tu.User.LastName,
				}).ToList(),
				Services = taskServices.Select(tu => new ServiceDto
				{
					Id = tu.TaskTemplateId,
					Name = tu.TaskTemplate.KeyName
				}).ToList(),
			};

			return Ok(data);
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
		[Route("add-task-comment")]
		public async Task<IActionResult> AddComment(int taskId, string comment)
		{
			var userId = UserId();
			var addComment = _taskService.AddComment(userId, taskId, comment);
            if (addComment == 0)
            {
				return Ok(0);
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

		[HttpPost]
		[Route("update-task")]
		public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskContent request)
		{
			string departman = string.Join(", ", request.Departments);

			var addTask = _taskService.UpdateTask(request.Id, request.Name, request.Content, request.Durations.ToUniversalTime(), departman);

			if (addTask == 0)
			{
				return Ok(0);
			}

			#region Users
			var existingUsers = (await _taskService.GetTaskUser(request.Id))
						.Select(tu => tu.UserId)
						.ToList();

			var usersToAdd = request.Users.Except(existingUsers).ToList();

			var usersToRemove = existingUsers.Except(request.Users).ToList();

			foreach (var userId in usersToAdd)
			{
				_taskService.AddTaskUser(userId, request.Id);
			}

			foreach (var userId in usersToRemove)
			{
				_taskService.IsDeletedTaskUser(request.Id, userId);
			}
			#endregion

			#region Services
			var existingServices = (await _taskService.GetTaskTemplateTask(request.Id))
						.Select(tu => tu.TaskTemplateId)
						.ToList();

			var servicesToAdd = request.Services.Except(existingServices).ToList();

			var servicesToRemove = existingServices.Except(request.Services).ToList();

			foreach (var serviceId in servicesToAdd)
			{
				_taskService.AddTaskTemplateTask(serviceId, request.Id);
			}

			foreach (var serviceId in servicesToRemove)
			{
				_taskService.IsDeletedTaskUser(request.Id, serviceId);
			}
			#endregion

			return Ok(1);
		}

		[HttpPost]
		[Route("update-task-state")]
		public async Task<IActionResult> UpdateTaskState(int taskId, int state)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var updateState = _taskService.UpdateTask(taskId, state);
            if (updateState == 0)
            {
				return Ok(0);
            }
            return Ok(1);
		}

		[HttpPost]
		[Route("update-task-template-task")]
		public async Task<IActionResult> UpdateTaskTemplateTask(int taskTemplateTaskId)
		{
			var updateState = _taskService.UpdateTaskTemplateTask(taskTemplateTaskId);
            if (updateState == 0)
            {
				return Ok(0);
            }
            return Ok(1);
		}

		[HttpPost]
		[Route("delete-task")]
		public async Task<IActionResult> DeleteTask(int taskId)
		{
			var deleteTask = _taskService.IsDeletedTask(taskId);
            if (deleteTask == 0)
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
