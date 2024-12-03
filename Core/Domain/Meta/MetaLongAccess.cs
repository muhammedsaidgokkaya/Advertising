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
        public int MetaAppId { get; set; }
        public virtual MetaApp MetaApp { get; set; }
        public int UserId { get; set; }
        public virtual Core.Domain.User.User User { get; set; }
    }
}
