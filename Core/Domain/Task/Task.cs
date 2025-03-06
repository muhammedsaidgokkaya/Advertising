using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Task
{
	public class Task : BaseEntity
	{
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int CreatedUser { get; set; }
        public string? Departments { get; set; }
        public DateTime? Deadline { get; set; }
        public int State { get; set; }
		public int OrganizationId { get; set; }
		public virtual Organization Organization { get; set; }
		public virtual ICollection<TaskUser> TaskUser { get; set; }
	}
}
