using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Google
{
    public class GoogleApp : BaseEntity
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string RedirectUrl { get; set; }
        public virtual ICollection<GoogleAccessToken> GoogleAccessToken { get; set; }
    }
}
