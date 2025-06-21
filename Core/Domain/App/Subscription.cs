using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.App
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PlanId { get; set; }
        public bool IsYearly { get; set; }
        public float Price { get; set; }
    }
}
