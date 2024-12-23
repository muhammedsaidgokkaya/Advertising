using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; }
        public int UserCount { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string TaskNumber { get; set; }
        public string Phone { get; set; }
        public virtual ICollection<User> User { get; set; }
    }
}
