using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Task
{
	public class TaskTemplate : BaseEntity
	{
        public string KeyName { get; set; }
		public int OrganizationId { get; set; }
		public virtual Organization Organization { get; set; }
	}
}
