using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Meta
{
    public class MetaAccess : BaseEntity
    {
        public string AccessToken { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public int UserId { get; set; }
        public virtual Core.Domain.User.User User { get; set; }
        public virtual ICollection<MetaLongAccess> MetaLongAccess { get; set; }
    }
}
