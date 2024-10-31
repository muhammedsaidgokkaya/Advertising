using Core.Data;
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

        public IPagedList<Core.Domain.User.User> GetPagedList(int pageIndex, int pageSize)
        {
            try
            {
                var returner = _repository.FilterAsQueryable<Core.Domain.User.User>(p => p.IsActive);
                return new PagedList<Core.Domain.User.User>(returner, pageIndex, pageSize);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
