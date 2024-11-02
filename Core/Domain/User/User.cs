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
        public virtual ICollection<MetaAccess> MetaAccess { get; set; }
    }
}
