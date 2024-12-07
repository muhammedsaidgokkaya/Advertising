using Core.Data;
using Core.Domain.Meta;
using Core.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Implementations;
using Service.Interfaces.User;
using Service.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations.User
{
    public class UserService : IUserService
    {
        private readonly Repository<Context> _repository;

        public UserService()
        {
            _repository = new Repository<Context>(new Context());
        }

        #region User
        public int AddUser(string name, string userName, string password)
        {
            var user = new Core.Domain.User.User
            {
                Name = name,
                UserName = userName,
                Password = password,
                InsertedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            _repository.Save(user);
            return user.Id;
        }

        public int UpdateUser(int id, string name, string userName, string password)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.Name = name;
                user.UserName = userName;
                user.Password = password;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public int IsActiveUser(int id)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public int IsDeletedMetaApp(int id)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.IsDeleted = !user.IsDeleted;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public Core.Domain.User.User GetUserById(int id)
        {
            return _repository.GetById<Core.Domain.User.User>(id);
        }

        public IEnumerable<Core.Domain.User.User> GetAll()
        {
            var data = _repository.Filter<Core.Domain.User.User>(p => p.IsActive);
            return data;
        }

        public int GetAllCount()
        {
            var data = _repository.Filter<Core.Domain.User.User>(p => p.IsActive).ToList();
            return data.Count();
        }

        public IEnumerable<Core.Domain.User.User> GetById(int id)
        {
            var data = _repository.Filter<Core.Domain.User.User>(p => p.IsActive && p.Id.Equals(id));
            return data;
        }

        public Core.Domain.User.User GetUserLogin(string userName, string password)
        {
            var data = _repository.FilterAsQueryable<Core.Domain.User.User>(p => p.IsActive)
                .IncludeUser()
                .FirstOrDefault(u => u.UserName == userName && u.Password == password); ;
            return data;
        }
        #endregion

        #region Role

        #endregion

        #region UserRole
        public IEnumerable<UserRole> GetUserRole(int userId)
        {
            var data = _repository.FilterAsQueryable<UserRole>(p => p.UserId.Equals(userId))
                .IncludeUserRole();
            return data;
        }
        #endregion
    }

    public static class UserExtensions
    {
        public static IQueryable<Core.Domain.User.User> IncludeUser(this IQueryable<Core.Domain.User.User> query)
        {
            return query
                .Include(ma => ma.UserRole);
        }

        public static IQueryable<UserRole> IncludeUserRole(this IQueryable<UserRole> query)
        {
            return query
                .Include(ma => ma.User)
                .Include(ma => ma.Role);
        }

        public static IQueryable<Role> IncludeRole(this IQueryable<Role> query)
        {
            return query
                .Include(ma => ma.UserRole);
        }
    }
}
