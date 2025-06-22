using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class PaymentFail : BaseEntity
    {
        public string Message { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
