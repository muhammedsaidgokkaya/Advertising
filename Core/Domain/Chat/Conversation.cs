using Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Chat
{
	public class Conversation : BaseEntity
	{
        public string Type { get; set; }
        public int UnreadCount { get; set; }
		public Guid? uId { get; set; }
		public int OrganizationId { get; set; }
		public virtual Core.Domain.User.Organization Organization { get; set; }
		public virtual ICollection<Message> Messages { get; set; }
		public virtual ICollection<Participant> Participants { get; set; }
	}
}
