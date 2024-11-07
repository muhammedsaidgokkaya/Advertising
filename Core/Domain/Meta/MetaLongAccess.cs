using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Meta
{
    public class MetaLongAccess : BaseEntity
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public int MetaAccessId { get; set; }
        public virtual MetaAccess MetaAccess { get; set; }
    }
}
