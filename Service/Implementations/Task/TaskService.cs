using Core.Data;
using Core.Domain.Task;
using Core.Domain.User;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
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

		//public int UpdateAdminUser(int id, string firstName, string lastName, string mail, string phone, string title, DateTime? dateOfBirth, string gender, string address)
		//{
		//	var user = GetUserById(id);
		//	if (user != null)
		//	{
		//		user.FirstName = firstName;
		//		user.LastName = lastName;
		//		user.Mail = mail;
		//		user.Phone = phone;
		//		user.Title = title;
		//		user.DateOfBirth = dateOfBirth;
		//		user.Gender = gender;
		//		user.Address = address;
		//		user.UpdateDate = DateTime.UtcNow;

		//		_repository.Update(user);
		//		return user.Id;
		//	}
		//	return 0;
		//}

		//public int IsActiveUser(int id)
		//{
		//	var user = GetUserById(id);
		//	if (user != null)
		//	{
		//		user.IsActive = !user.IsActive;
		//		user.UpdateDate = DateTime.UtcNow;

		//		_repository.Update(user);
		//		return user.Id;
		//	}
		//	return 0;
		//}

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

		public TaskTemplate GetTaskTemplateById(int id)
		{
			return _repository.GetById<TaskTemplate>(id);
		}

		public async Task<IEnumerable<TaskTemplate>> GetTaskTemplate(int organizationId)
		{
			var data = _repository.FilterAsQueryable<TaskTemplate>(p => !p.IsDeleted && p.Organization.Id.Equals(organizationId)).IncludeTaskTemplate();
			return data;
		}

		//public IEnumerable<Core.Domain.User.User> GetUsers(int organizationId, int userId)
		//{
		//	var data = _repository
		//		.FilterAsQueryable<Core.Domain.User.User>(
		//			p => !p.IsDeleted
		//				 && p.Organization.Id.Equals(organizationId)
		//				 && !p.Id.Equals(userId))
		//		.IncludeUser();
		//	return data;
		//}
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
