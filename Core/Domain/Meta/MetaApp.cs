using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Meta
{
    public class MetaApp : BaseEntity
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public virtual ICollection<MetaLongAccess> MetaLongAccess { get; set; }
    }
}
