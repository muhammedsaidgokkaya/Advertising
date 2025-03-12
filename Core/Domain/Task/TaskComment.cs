using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Task
{
	public class TaskComment : BaseEntity
	{
        public string Comment { get; set; }
		public int TaskId { get; set; }
		public virtual Task Task { get; set; }
		public int UserId { get; set; }
		public virtual User.User User { get; set; }
	}
}
