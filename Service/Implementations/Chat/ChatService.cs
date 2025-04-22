using Core.Data;
using Core.Domain.Chat;
using Core.Domain.Task;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Service.Interfaces.Chat;
using Service.Interfaces.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Service.Implementations.Chat
{
	public class ChatService : IChatService
	{
		private readonly Repository<Context> _repository;

		public ChatService()
		{
			_repository = new Repository<Context>(new Context());
		}

		public async Task<int> AddConversation(int organizationId, Guid? uId, string type, int unreadCount)
		{
			var conversation = new Conversation
			{
				uId = uId,
				UnreadCount = unreadCount,
				Type = type,
				OrganizationId = organizationId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			await _repository.SaveAsync(conversation);
			return conversation.Id;
		}

		public async Task<int> AddAttachment(int messageId, Guid? uId, string name, string path, string preview, long size, string type)
		{
			var attachment = new Core.Domain.Chat.Attachment
			{
				Name = name,
				Path = path,
				Preview = preview,
				Size = size,
				Type = type,
				uId = uId,
				MessageId = messageId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			await _repository.SaveAsync(attachment);
			return attachment.Id;
		}

		public async Task<int> AddMessage(Guid? uId, int senderId, int conversationId, string body, string contentType)
		{
			var message = new Message
			{
				Body = body,
				ContentType = contentType,
				uId = uId,
				SenderId = senderId,
				ConversationId = conversationId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			await _repository.SaveAsync(message);
			return message.Id;
		}

		public async Task<int> AddParticipant(int userId, int conversationId)
		{
			var participant = new Participant
			{
				UserId = userId,
				ConversationId = conversationId,
				InsertedDate = DateTime.UtcNow,
				IsActive = true,
				IsDeleted = false
			};

			await _repository.SaveAsync(participant);
			return participant.Id;
		}

		public Conversation GetConversation(Guid conversationId)
		{
			var data = _repository.FilterAsQueryable<Conversation>(p => !p.IsDeleted && p.uId.Equals(conversationId))
								  .IncludeConversation()
								  .FirstOrDefault();
			return data;
		}

		public async Task<int> GetConversationId(Guid conversationId)
		{
			var data = await _repository.FilterAsQueryable<Conversation>(p => !p.IsDeleted && p.uId.Equals(conversationId))
										 .IncludeConversation()
										 .FirstOrDefaultAsync();
			return data?.Id ?? 0;
		}

		public async Task<IEnumerable<Conversation>> GetConversations(int organizationId, int userId)
		{
			var data = _repository
				.FilterAsQueryable<Conversation>(p =>
					!p.IsDeleted &&
					p.Organization.Id == organizationId &&
					p.Participants.Any(participant => participant.User.Id == userId)
				)
				.IncludeConversation();

			return data;
		}
	}

	public static class ChatExtensions
	{
		public static IQueryable<Conversation> IncludeConversation(this IQueryable<Conversation> query)
		{
			return query
				.Include(ma => ma.Messages)
				.ThenInclude(m => m.Attachments)
				.Include(ma => ma.Organization)
				.Include(ma => ma.Participants)
				.ThenInclude(m => m.User);
		}

		public static IQueryable<Message> IncludeMessage(this IQueryable<Message> query)
		{
			return query
				.Include(ma => ma.Conversation)
				.Include(ma => ma.Attachments);
		}

		public static IQueryable<Participant> IncludeParticipant(this IQueryable<Participant> query)
		{
			return query
				.Include(ma => ma.User)
				.Include(ma => ma.Conversation);
		}

		public static IQueryable<Core.Domain.Chat.Attachment> IncludeAttachment(this IQueryable<Core.Domain.Chat.Attachment> query)
		{
			return query
				.Include(ma => ma.Message);
		}
	}
}
