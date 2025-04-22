using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Chat
{
	public class Message : BaseEntity
	{
        public string Body { get; set; }
        public string ContentType { get; set; }
		public Guid? uId { get; set; }
		public int SenderId { get; set; }
		public int ConversationId { get; set; }
		public virtual Conversation Conversation { get; set; }
		public virtual ICollection<Attachment> Attachments { get; set; }
	}
}
