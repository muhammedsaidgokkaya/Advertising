using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class Plan : BaseEntity
    {
        public int Amount { get; set; }
        public int PlanId { get; set; }
        public bool IsYearly { get; set; }
        public bool IsPayment { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
