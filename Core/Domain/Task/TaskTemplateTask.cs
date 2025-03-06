using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Task
{
	public class TaskTemplateTask : BaseEntity
	{
		public int TaskTemplateId { get; set; }
		public virtual TaskTemplate TaskTemplate { get; set; }
		public int TaskId { get; set; }
		public virtual Task Task { get; set; }
	}
}
