using Core.Data;
using Core.Domain.Task;
using Core.Domain.User;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Service.Implementations.User;
using Service.Interfaces.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations.Task
{
	public class TaskService : ITaskService
	{
		private readonly Repository<Context> _repository;

		public TaskService()
		{
			_repository = new Repository<Context>(new Context());
		}

		public int AddTaskTemplate(int organizationId, string name)
		{
			var taskTemplate = new TaskTemplate
			{
				KeyName = name,
				OrganizationId = organizationId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			_repository.Save(taskTemplate);
			return taskTemplate.Id;
		}

		public int AddTask(int createdUser, int organizationId, string name, string content, DateTime duration, string departmans, int priority)
		{
			var task = new Core.Domain.Task.Task
			{
				TaskName = name,
				Description = content,
				Deadline = duration,
				Departments = departmans,
				OrganizationId = organizationId,
				CreatedUser = createdUser,
				Priority = priority,
				State = 0,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			_repository.Save(task);
			return task.Id;
		}

		public int AddComment(int createdUser, int taskId, string comment)
		{
			var taskComment = new Core.Domain.Task.TaskComment
			{
				UserId = createdUser,
				TaskId = taskId,
				Comment = comment,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			_repository.Save(taskComment);
			return taskComment.Id;
		}

		public int AddTaskUser(int userId, int taskId)
		{
			var taskUser = new TaskUser
			{
				UserId = userId,
				TaskId = taskId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			_repository.Save(taskUser);
			return 1;
		}

		public int AddTaskTemplateTask(int taskTemplateId, int taskId)
		{
			var taskTemplate = new TaskTemplateTask
			{
				TaskTemplateId = taskTemplateId,
				TaskId = taskId,
				IsFinished = false,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			_repository.Save(taskTemplate);
			return 1;
		}

		public int UpdateTask(int id, string name, string content, DateTime duration, string departmans, int priority)
		{
			var task = GetTaskById(id);
			if (task != null)
			{
				task.TaskName = name;
				task.Description = content;
				task.Deadline = duration;
				task.Departments = departmans;
				task.Priority = priority;
				task.UpdateDate = DateTime.UtcNow;

				_repository.Update(task);
				return task.Id;
			}
			return 0;
		}

		public int IsDeletedTaskTemplate(int id)
		{
			var taskTemplate = GetTaskTemplateById(id);
			if (taskTemplate != null)
			{
				taskTemplate.IsDeleted = !taskTemplate.IsDeleted;
				taskTemplate.UpdateDate = DateTime.UtcNow;

				_repository.Update(taskTemplate);
				return taskTemplate.Id;
			}
			return 0;
		}

		public int IsDeletedTaskTemplateTask(int id)
		{
			var taskTemplateTask = GetTaskTemplateTaskById(id);
			if (taskTemplateTask != null)
			{
				taskTemplateTask.IsDeleted = !taskTemplateTask.IsDeleted;
				taskTemplateTask.UpdateDate = DateTime.UtcNow;

				_repository.Update(taskTemplateTask);
				return taskTemplateTask.Id;
			}
			return 0;
		}

		public int IsDeletedTask(int id)
		{
			var task = GetTaskById(id);
			if (task != null)
			{
				task.IsDeleted = !task.IsDeleted;
				task.UpdateDate = DateTime.UtcNow;

				_repository.Update(task);
				return task.Id;
			}
			return 0;
		}

		public int IsDeletedTaskUser(int id)
		{
			var taskUser = GetTaskUserById(id);
			if (taskUser != null)
			{
				taskUser.IsDeleted = !taskUser.IsDeleted;
				taskUser.UpdateDate = DateTime.UtcNow;

				_repository.Update(taskUser);
				return taskUser.Id;
			}
			return 0;
		}

		public int IsDeletedTaskUser(int taskId, int userId)
		{
			var taskUser = _repository.Find<TaskUser>(tu => tu.TaskId == taskId && tu.UserId == userId);

			if (taskUser != null)
			{
				taskUser.IsDeleted = !taskUser.IsDeleted;
				taskUser.UpdateDate = DateTime.UtcNow;

				_repository.Update(taskUser);
				return taskUser.Id;
			}

			return 0;
		}

		public int IsDeletedTaskTemplateTask(int taskId, int taskTemplateId)
		{
			var taskTemplateTask = _repository.Find<TaskTemplateTask>(tu => tu.TaskId == taskId && tu.TaskTemplateId == taskTemplateId);

			if (taskTemplateTask != null)
			{
				taskTemplateTask.IsDeleted = !taskTemplateTask.IsDeleted;
				taskTemplateTask.UpdateDate = DateTime.UtcNow;

				_repository.Update(taskTemplateTask);
				return taskTemplateTask.Id;
			}

			return 0;
		}

		public int UpdateTask(int id, int state)
		{
			var task = GetTaskById(id);
			if (task != null)
			{
				task.State = state;
				task.UpdateDate = DateTime.UtcNow;

				_repository.Update(task);
				return task.Id;
			}
			return 0;
		}

		public bool UpdateTaskTemplateTask(int id)
		{
			var taskTemplateTask = GetTaskTemplateTaskById(id);
			if (taskTemplateTask != null)
			{
				taskTemplateTask.IsFinished = !taskTemplateTask.IsFinished;
				taskTemplateTask.UpdateDate = DateTime.UtcNow;

				_repository.Update(taskTemplateTask);
				return taskTemplateTask.IsFinished;
			}
			return false;
		}

		public TaskTemplate GetTaskTemplateById(int id)
		{
			return _repository.GetById<TaskTemplate>(id);
		}

		public TaskTemplateTask GetTaskTemplateTaskById(int id)
		{
			return _repository.GetById<TaskTemplateTask>(id);
		}

		public TaskComment GetTaskCommentById(int id)
		{
			return _repository.GetById<TaskComment>(id);
		}

		public Core.Domain.Task.Task GetTaskById(int id)
		{
			return _repository.GetById<Core.Domain.Task.Task>(id);
		}

		public Core.Domain.Task.TaskUser GetTaskUserById(int id)
		{
			return _repository.GetById<Core.Domain.Task.TaskUser>(id);
		}

		public async Task<IEnumerable<TaskTemplate>> GetTaskTemplate(int organizationId)
		{
			var data = _repository.FilterAsQueryable<TaskTemplate>(p => !p.IsDeleted && p.Organization.Id.Equals(organizationId)).IncludeTaskTemplate();
			return data;
		}

		public async Task<IEnumerable<TaskTemplateTask>> GetTaskTemplateTask(int taskId)
		{
			var data = _repository.FilterAsQueryable<TaskTemplateTask>(p => !p.IsDeleted && !p.TaskTemplate.IsDeleted && p.Task.Id.Equals(taskId)).IncludeTaskTemplateTask();
			return data;
		}

		public TaskTemplateTask GetTaskTemplateTaskTask(int taskTemplateTaskId)
		{
			var data = _repository.FilterAsQueryable<TaskTemplateTask>(p => !p.IsDeleted && p.Id.Equals(taskTemplateTaskId))
								  .IncludeTaskTemplateTask()
								  .FirstOrDefault();
			return data;
		}

		public async Task<IEnumerable<TaskUser>> GetTaskUser(int taskId)
		{
			var data = _repository.FilterAsQueryable<TaskUser>(p => !p.IsDeleted && p.Task.Id.Equals(taskId)).IncludeTaskUser();
			return data;
		}

		public async Task<IEnumerable<TaskComment>> GetTaskComment(int taskId)
		{
			var data = _repository.FilterAsQueryable<TaskComment>(p => !p.IsDeleted && p.Task.Id.Equals(taskId)).IncludeTaskComment();
			return data;
		}

		public async Task<IEnumerable<Core.Domain.Task.Task>> GetTasks(int organizationId)
		{
			var data = _repository.FilterAsQueryable<Core.Domain.Task.Task>(p => !p.IsDeleted && p.Organization.Id.Equals(organizationId)).IncludeTask();
			return data;
		}

		public int GetTasksCount(int organizationId)
		{
			var data = _repository.FilterAsQueryable<Core.Domain.Task.Task>(p => !p.IsDeleted && p.Organization.Id.Equals(organizationId)).IncludeTask().Count();
			return data;
		}
	}

	public static class TaskExtensions
	{
		public static IQueryable<Core.Domain.Task.Task> IncludeTask(this IQueryable<Core.Domain.Task.Task> query)
		{
			return query
				.Include(ma => ma.TaskUser)
				.Include(ma => ma.TaskTemplateTask)
				.Include(ma => ma.TaskComment);
		}

		public static IQueryable<TaskUser> IncludeTaskUser(this IQueryable<TaskUser> query)
		{
			return query
				.Include(ma => ma.User)
				.Include(ma => ma.Task);
		}

		public static IQueryable<TaskComment> IncludeTaskComment(this IQueryable<TaskComment> query)
		{
			return query
				.Include(ma => ma.User)
				.Include(ma => ma.Task);
		}

		public static IQueryable<TaskTemplate> IncludeTaskTemplate(this IQueryable<TaskTemplate> query)
		{
			return query
				.Include(ma => ma.Organization);
		}

		public static IQueryable<TaskTemplateTask> IncludeTaskTemplateTask(this IQueryable<TaskTemplateTask> query)
		{
			return query
				.Include(ma => ma.Task)
				.Include(ma => ma.TaskTemplate);
		}

		public static IQueryable<Core.Domain.User.User> IncludeUser(this IQueryable<Core.Domain.User.User> query)
		{
			return query
				.Include(ma => ma.TaskUser)
				.Include(ma => ma.TaskComment)
				.Include(ma => ma.Organization);
		}
	}
}
