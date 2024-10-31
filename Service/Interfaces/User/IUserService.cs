using Core.Data;
using Service.Implementations.User;
using Service.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces.User
{
    interface IUserService
    {
        IEnumerable<Core.Domain.User.User> GetAll();
        IPagedList<Core.Domain.User.User> GetPagedList(int pageIndex, int pageSize);
    }
}
