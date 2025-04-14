using AdminPanel.Controllers.Organization;
using AdminPanel.Helpers.ExcelService;
using AdminPanel.Models.Organization.User;
using AdminPanel.Models.Task.Task;
using AdminPanel.Models.Task.TaskTemplate;
using Core.Domain.Task;
using Core.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Service.Implementations.Task;
using Service.Implementations.User;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
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
		private readonly ExcelService _excelService;
		private readonly DefaultValues _defaultValues;
		private readonly EmailHelper _emailHelper;

		public TaskController(ILogger<TaskController> logger)
		{
			_logger = logger;
			_userService = new UserService();
			_taskService = new TaskService();
			_excelService = new ExcelService();
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
					Priority = task.Priority,
					CreatedUser = createdUser != null ? $"{createdUser.FirstName} {createdUser.LastName}" : "Bilinmiyor",
					Duration = task.Deadline ?? DateTime.MinValue,
					Team = task.TaskUser.Count,
				};
			})
			.OrderByDescending(task => task.Priority)
			.ThenBy(task => task.Duration)
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
				Priority = task.Priority,
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
				Priority = task.Priority,
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
				var addTemplate = _taskService.AddTaskTemplate(user.OrganizationId, template);
                if (addTemplate != 0)
                {
					Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"Process\", \"TaskTemplateId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + ", 11, " + addTemplate + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
                }
            }

			foreach (var template in templatesToRemove)
			{
				var deleteTemplate = _taskService.IsDeletedTaskTemplate(template);
				if (deleteTemplate != 0)
				{
					Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"Process\", \"TaskTemplateId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + ", 12, " + deleteTemplate + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
				}
			}

			return Ok(1);
		}

		[HttpPost]
		[Route("add-task-comment")]
		public async Task<IActionResult> AddComment(int taskId, string comment)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var addComment = _taskService.AddComment(userId, taskId, comment);
            if (addComment != 0)
            {
				Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"CommentId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + taskId + ", 13, " + addComment + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
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

			var addTask = _taskService.AddTask(userId, user.OrganizationId, request.Name, request.Content, request.Durations.ToUniversalTime(), departman, request.Priority);
            
			if (addTask == 0)
            {
				return Ok(0);
            }

			Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + addTask + ", 15, '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");

			if (request.Users != null || request.Users.Count != 0 )
            {
				foreach (var item in request.Users)
				{
					_taskService.AddTaskUser(item, addTask);
					Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TransactionUser\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + addTask + ", 5, " + item + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
				}
			}

            if (request.Services != null || request.Services.Count != 0 )
            {
				foreach (var item in request.Services)
				{
					_taskService.AddTaskTemplateTask(item, addTask);
					Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + addTask + ", 7, " + item + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
				}
			}
            
            return Ok(1);
		}

		[HttpPost]
		[Route("update-task")]
		public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskContent request)
		{
			var uId = UserId();
			var user = _userService.GetUserById(uId);
			string departman = string.Join(", ", request.Departments);

			var addTask = _taskService.UpdateTask(request.Id, request.Name, request.Content, request.Durations.ToUniversalTime(), departman, request.Priority);

			if (addTask == 0)
			{
				return Ok(0);
			}

			Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"InsertedDate\", \"OrganizationId\") VALUES (" + uId + " , " + addTask + ", 16, '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
			
			#region Users
			var existingUsers = (await _taskService.GetTaskUser(request.Id))
						.Select(tu => tu.UserId)
						.ToList();

			var usersToAdd = request.Users.Except(existingUsers).ToList();

			var usersToRemove = existingUsers.Except(request.Users).ToList();

			foreach (var userId in usersToAdd)
			{
				_taskService.AddTaskUser(userId, request.Id);
				Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TransactionUser\", \"InsertedDate\", \"OrganizationId\") VALUES (" + uId + " , " + addTask + ", 5, " + userId + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
			}

			foreach (var userId in usersToRemove)
			{
				_taskService.IsDeletedTaskUser(request.Id, userId);
				Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TransactionUser\", \"InsertedDate\", \"OrganizationId\") VALUES (" + uId + " , " + addTask + ", 6, " + userId + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
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
				Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + uId + " , " + addTask + ", 7, " + serviceId + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
			}

			foreach (var serviceId in servicesToRemove)
			{
				_taskService.IsDeletedTaskUser(request.Id, serviceId);
				Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + uId + " , " + addTask + ", 8, " + serviceId + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
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

			Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + taskId + ", " + state + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
			return Ok(1);
		}

		[HttpPost]
		[Route("update-task-template-task")]
		public async Task<IActionResult> UpdateTaskTemplateTask(int taskTemplateTaskId)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var updateState = _taskService.UpdateTaskTemplateTask(taskTemplateTaskId);
			var task = _taskService.GetTaskTemplateTaskTask(taskTemplateTaskId);

			if (updateState)
            {
				Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + task.TaskId + ", 9, " + taskTemplateTaskId + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
			}
            else
            {
				Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + task.TaskId + ", 10, " + taskTemplateTaskId + ", '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
			}
			return Ok(1);
		}

		[HttpPost]
		[Route("delete-task")]
		public async Task<IActionResult> DeleteTask(int taskId)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var deleteTask = _taskService.IsDeletedTask(taskId);
            if (deleteTask == 0)
            {
				return Ok(0);
            }
			Sql.GetQueryResult("INSERT INTO public.\"TaskLog\"(\"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"InsertedDate\", \"OrganizationId\") VALUES (" + userId + " , " + deleteTask + ", 13, '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz") + ", " + user.OrganizationId + "');");
			return Ok(1);
		}

		[HttpGet("user-count")]
		public async Task<IActionResult> ReportUserCount()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var result = Sql.GetQueryResult("SELECT \"UserPerformingTheTransaction\", COUNT(*) AS transaction_count FROM public.\"TaskLog\" WHERE \"OrganizationId\" = " + user.OrganizationId + " GROUP BY \"UserPerformingTheTransaction\" ORDER BY transaction_count DESC LIMIT 3;");
			var response = new List<object>();

			foreach (DataRow row in result.Rows)
			{
				var userPerformingId = Convert.ToInt32(row["UserPerformingTheTransaction"]);
				var performingUser = _userService.GetUserById(userPerformingId);


				if (performingUser != null)
				{
					response.Add(new
					{
						Id = performingUser.Id,
						Name = performingUser.FirstName + " " + performingUser.LastName,
					});
				}
			}

			return Ok(response);
		}

		[HttpGet("user-apex")]
		public async Task<IActionResult> ReportUserApex()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var result = Sql.GetQueryResult("SELECT \"UserPerformingTheTransaction\", COUNT(*) AS transaction_count FROM public.\"TaskLog\" WHERE \"OrganizationId\" = " + user.OrganizationId + " GROUP BY \"UserPerformingTheTransaction\" ORDER BY transaction_count DESC LIMIT 3;");
			var response = new List<object>();
			int totalTransactions = 0;

			foreach (DataRow row in result.Rows)
			{
				totalTransactions += Convert.ToInt32(row["transaction_count"]);
			}

			foreach (DataRow row in result.Rows)
			{
				var userPerformingId = Convert.ToInt32(row["UserPerformingTheTransaction"]);
				var performingUser = _userService.GetUserById(userPerformingId);

				if (performingUser != null)
				{
					var transactionCount = Convert.ToInt32(row["transaction_count"]);
					var percentage = totalTransactions > 0 ? (transactionCount / (float)totalTransactions) * 100 : 0;

					response.Add(new
					{
						Id = performingUser.Id,
						Name = performingUser.FirstName + " " + performingUser.LastName,
						Count = transactionCount,
						Percentage = percentage.ToString("00")
					});
				}
			}

			return Ok(new { Total = totalTransactions, Users = response });
		}

		[HttpGet("task-list-report")]
		public async Task<IActionResult> ReportTaskList()
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
					CreatedUser = createdUser != null ? $"{createdUser.FirstName} {createdUser.LastName}" : "Bilinmiyor",
				};
			})
			.OrderByDescending(task => task.CreatedDate)
			.Take(5)
			.ToList();

			return Ok(taskList);
		}

		[HttpGet("task-count-report")]
		public async Task<IActionResult> ReportTaskCount()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var tasks = await _taskService.GetTasks(user.OrganizationId);

			var taskCounts = tasks
				.GroupBy(t => t.State)
				.ToDictionary(g => g.Key, g => g.Count());

			var totalCount = tasks.Count();

			double GetPercentage(int count) => totalCount > 0 ? Math.Round((count / (double)totalCount) * 100, 2) : 0;

			var result = new
			{
				Bekleyen = new { Count = taskCounts.GetValueOrDefault(0, 0), Percentage = GetPercentage(taskCounts.GetValueOrDefault(0, 0)) },
				DevamEden = new { Count = taskCounts.GetValueOrDefault(1, 0), Percentage = GetPercentage(taskCounts.GetValueOrDefault(1, 0)) },
				Tamamlanan = new { Count = taskCounts.GetValueOrDefault(2, 0), Percentage = GetPercentage(taskCounts.GetValueOrDefault(2, 0)) },
				IptalEdilen = new { Count = taskCounts.GetValueOrDefault(3, 0), Percentage = GetPercentage(taskCounts.GetValueOrDefault(3, 0)) },
				Toplam = totalCount
			};

			return Ok(result);
		}

		[HttpGet("tasks-chart-report")]
		public async Task<ActionResult<IEnumerable<Models.Task.Task.Tasks>>> GetTasksChart()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var tasks = await _taskService.GetTasks(user.OrganizationId);

			var taskList = tasks.Select(task =>
			{
				return new Models.Task.Task.Tasks
				{
					Id = task.Id,
					Name = task.TaskName,
					Team = task.TaskUser.Count,
				};
			})
			.ToList();

			return Ok(taskList);
		}

		[HttpGet("all-report")]
		public IActionResult AllReportExcel()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var result = Sql.GetQueryResult("SELECT \"Id\", \"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"TransactionUser\", \"CommentId\", \"TaskTemplateId\", \"InsertedDate\", \"OrganizationId\" FROM public.\"TaskLog\" WHERE \"OrganizationId\" = " + user.OrganizationId + ";");

			return _excelService.CreateExcelFile(result);
		}

		[HttpGet("task-report")]
		public IActionResult TaskReportExcel(int taskId)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var result = Sql.GetQueryResult("SELECT \"Id\", \"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"TransactionUser\", \"CommentId\", \"TaskTemplateId\", \"InsertedDate\", \"OrganizationId\" FROM public.\"TaskLog\" WHERE \"OrganizationId\" = " + user.OrganizationId + " AND \"TaskId\" = " + taskId + ";");

			return _excelService.CreateExcelFile(result);
		}

		[HttpGet("user-report")]
		public IActionResult UserReportExcel(int uId)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var result = Sql.GetQueryResult("SELECT \"Id\", \"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"TransactionUser\", \"CommentId\", \"TaskTemplateId\", \"InsertedDate\", \"OrganizationId\" FROM public.\"TaskLog\" WHERE \"OrganizationId\" = " + user.OrganizationId + " AND \"UserPerformingTheTransaction\" = " + uId + ";");

			return _excelService.CreateExcelFile(result);
		}

		[HttpGet("task-user-report")]
		public IActionResult TaskUserReportExcel(int taskId, int uId)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var result = Sql.GetQueryResult("SELECT \"Id\", \"UserPerformingTheTransaction\", \"TaskId\", \"Process\", \"TaskTemplateTaskId\", \"TransactionUser\", \"CommentId\", \"TaskTemplateId\", \"InsertedDate\", \"OrganizationId\" FROM public.\"TaskLog\" WHERE \"OrganizationId\" = " + user.OrganizationId + " AND \"TaskId\" = " + taskId + " AND \"UserPerformingTheTransaction\" = " + uId + ";");

			return _excelService.CreateExcelFile(result);
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
