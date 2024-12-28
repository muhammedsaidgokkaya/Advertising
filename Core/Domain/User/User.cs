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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Photo { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? DarkMode { get; set; }
        public string? Contrast { get; set; }
        public string? RightToLeft { get; set; }
        public string? Compact { get; set; }
        public string? Presets { get; set; }
        public string? Layout { get; set; }
        public string? Family { get; set; }
        public string? Size { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual ICollection<GoogleAccessToken> GoogleAccessToken { get; set; }
        public virtual ICollection<MetaLongAccess> MetaLongAccess { get; set; }
        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}
