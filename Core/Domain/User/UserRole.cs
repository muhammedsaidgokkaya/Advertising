using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class UserRole
    {
        public int UserId { get; set; }
        public virtual Core.Domain.User.User User { get; set; }
        public int RoleId { get; set; }
        public virtual Core.Domain.User.Role Role { get; set; }
    }
}
