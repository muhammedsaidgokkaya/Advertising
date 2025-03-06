using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Task
{
	public class TaskUser : BaseEntity
	{
		public int UserId { get; set; }
		public virtual Core.Domain.User.User User { get; set; }
		public int TaskId { get; set; }
		public virtual Task Task { get; set; }
	}
}
