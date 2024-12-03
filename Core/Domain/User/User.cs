using Core.Domain.Google;
using Core.Domain.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public virtual ICollection<GoogleAccessToken> GoogleAccessToken { get; set; }
        public virtual ICollection<MetaLongAccess> MetaLongAccess { get; set; }
        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}
