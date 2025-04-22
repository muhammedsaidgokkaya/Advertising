using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Chat
{
	public class Attachment : BaseEntity
	{
		public string Name { get; set; }
        public string Path { get; set; }
        public string Preview { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
		public Guid? uId { get; set; }
		public int MessageId { get; set; }
		public virtual Message Message { get; set; }
	}
}
