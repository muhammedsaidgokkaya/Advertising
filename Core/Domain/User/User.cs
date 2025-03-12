using Core.Domain.Google;
using Core.Domain.Meta;
using Core.Domain.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual ICollection<GoogleAccessToken> GoogleAccessToken { get; set; }
        public virtual ICollection<MetaLongAccess> MetaLongAccess { get; set; }
        public virtual ICollection<UserRole> UserRole { get; set; }
		public virtual ICollection<TaskUser> TaskUser { get; set; }
		public virtual ICollection<TaskComment> TaskComment { get; set; }
	}
}
