using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Meta
{
    public class MetaLongAccess : BaseEntity
    {
        public int MetaAccessId { get; set; }
        public virtual MetaAccess MetaAccess { get; set; }
    }
}
