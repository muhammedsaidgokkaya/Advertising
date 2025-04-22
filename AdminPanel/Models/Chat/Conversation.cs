namespace AdminPanel.Models.Chat
{
	public class Conversation
	{
		public Guid? Id { get; set; }
		public string Type { get; set; }
		public int UnreadCount { get; set; }
		public List<Message> Messages { get; set; }
		public List<Participant> Participants { get; set; }
	}

	public class Message
	{
		public Guid? Id { get; set; }
		public int SenderId { get; set; }
		public string Body { get; set; }
		public string ContentType { get; set; }
		public DateTime? CreatedAt { get; set; }
		public List<Attachment> Attachments { get; set; }
	}

	public class Attachment
	{
		public Guid? Id { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string Preview { get; set; }
		public string Type { get; set; }
		public long Size { get; set; }
	}

	public class Participant
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string Role { get; set; }
		public string Status { get; set; }
		public string AvatarUrl { get; set; }
		public string Address { get; set; }
		public DateTime? LastActivity { get; set; }
	}

	public class PostMessageRequest
	{
		public Guid ConversationId { get; set; }
		public Message MessageData { get; set; }
	}
}
