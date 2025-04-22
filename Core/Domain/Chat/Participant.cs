using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Chat
{
	public class Participant : BaseEntity
	{
		public int UserId { get; set; }
		public virtual Core.Domain.User.User User { get; set; }
		public int ConversationId { get; set; }
		public virtual Conversation Conversation { get; set; }
	}
}
