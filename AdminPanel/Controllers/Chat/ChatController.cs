using AdminPanel.Controllers.Calendar;
using AdminPanel.Models.Chat;
using AdminPanel.Models.Organization.User;
using AdminPanel.Models.Task.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations.Calendar;
using Service.Implementations.Chat;
using Service.Implementations.User;
using Utilities.Helper;

namespace AdminPanel.Controllers.Chat
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class ChatController : ControllerBase
	{
		private readonly ILogger<ChatController> _logger;
		private readonly UserService _userService;
		private readonly ChatService _chatService;
		private readonly DefaultValues _defaultValues;
		private readonly EmailHelper _emailHelper;

		public ChatController(ILogger<ChatController> logger)
		{
			_logger = logger;
			_userService = new UserService();
			_chatService = new ChatService();
			_defaultValues = new DefaultValues();
			_emailHelper = new EmailHelper();
		}

		[HttpGet("conversations")]
		public async Task<ActionResult<IEnumerable<Conversation>>> GetConversations()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var conversations = await _chatService.GetConversations(user.OrganizationId, userId);

			var conversationList = conversations.Select(conversation => new Conversation
			{
				Id = conversation.uId,
				Type = conversation.Type,
				UnreadCount = conversation.UnreadCount,
				Messages = conversation.Messages.Select(m => new AdminPanel.Models.Chat.Message
				{
					Id = m.uId,
					SenderId = m.SenderId,
					Body = m.Body,
					ContentType = m.ContentType,
					CreatedAt = m.InsertedDate,
					Attachments = m.Attachments.Select(a => new AdminPanel.Models.Chat.Attachment
					{
						Id = a.uId,
						Name = a.Name,
						Path = a.Path,
						Preview = a.Preview,
						Type = a.Type,
						Size = a.Size
					}).ToList()
				}).ToList(),
				Participants = conversation.Participants.Select(p =>
				{
					var user = _userService.GetUserById(p.UserId);
					return new Participant
					{
						Id = user.Id,
						Name = user.FirstName + " " + user.LastName,
						Email = user.Mail,
						AvatarUrl = "/user/" + user.Id + ".png",
						Status = user.ActivityStatus,
						PhoneNumber = user.Phone,
						Role = user.Title,
						Address = user.Address,
						LastActivity = user.LastActivity,
					};
				}).ToList()
			}).ToList();

			return Ok(conversationList);
		}

		[HttpGet("conversation")]
		public async Task<ActionResult<Conversation>> GetConversation(Guid conversationId)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var conversation = _chatService.GetConversation(conversationId);

			var data = new Conversation
			{
				Id = conversation.uId,
				Type = conversation.Type,
				UnreadCount = conversation.UnreadCount,
				Messages = conversation.Messages.Select(m => new AdminPanel.Models.Chat.Message
				{
					Id = m.uId,
					SenderId = m.SenderId,
					Body = m.Body,
					ContentType = m.ContentType,
					CreatedAt = m.InsertedDate,
					Attachments = m.Attachments.Select(a => new AdminPanel.Models.Chat.Attachment
					{
						Id = a.uId,
						Name = a.Name,
						Path = a.Path,
						Preview = a.Preview,
						Type = a.Type,
						Size = a.Size
					}).ToList()
				}).ToList(),
				Participants = conversation.Participants.Select(p =>
				{
					var user = _userService.GetUserById(p.UserId);
					return new Participant
					{
						Id = user.Id,
						Name = user.FirstName + " " + user.LastName,
						Email = user.Mail,
						AvatarUrl = "/user/" + user.Id + ".png",
						Status = user.ActivityStatus,
						PhoneNumber = user.Phone,
						Role = user.Title,
						Address = user.Address,
						LastActivity = user.LastActivity,
					};
				}).ToList()
			};

			return Ok(data);
		}

		[HttpGet("users")]
		public ActionResult<IEnumerable<Participant>> GetUsers()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);
			var users = _userService.GetUsers(user.OrganizationId, userId);

			var userList = users.Select(user => new Participant
			{
				Id = user.Id,
				Name = user.FirstName + " " + user.LastName,
				Email = user.Mail,
				AvatarUrl = "/user/" + user.Id + ".png",
				Status = user.ActivityStatus,
				PhoneNumber = user.Phone,
				Role = user.Title,
				Address = user.Address,
				LastActivity = user.LastActivity,
			}).ToList();

			return Ok(userList);
		}

		[HttpGet("user")]
		public ActionResult<Participant> GetUser()
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);

			var userList = new Participant
			{
				Id = user.Id,
				Name = user.FirstName + " " + user.LastName,
				Email = user.Mail,
				AvatarUrl = "/user/" + user.Id + ".png",
				Status = user.ActivityStatus,
				PhoneNumber = user.Phone,
				Role = user.Title,
				Address = user.Address,
				LastActivity = user.LastActivity,
			};

			return Ok(userList);
		}

		[HttpPost]
		[Route("add-conversation")]
		public async Task<IActionResult> PostConversation([FromBody] Conversation request)
		{
			var userId = UserId();
			var user = _userService.GetUserById(userId);

			var addConversation = await _chatService.AddConversation(user.OrganizationId, request.Id, request.Type, request.UnreadCount);

			if (addConversation != 0)
			{
				foreach (var item in request.Participants)
				{
					await _chatService.AddParticipant(item.Id, addConversation);
				}

				var firstMessage = request.Messages.FirstOrDefault();
				if (firstMessage != null)
				{
					var messageUId = firstMessage.Id;
					var body = firstMessage.Body;
					var contentType = firstMessage.ContentType;
					var addMessage = await _chatService.AddMessage(messageUId, userId, addConversation, body, contentType);

					if (addMessage != 0)
					{
						if (firstMessage.Attachments != null)
						{
							foreach (var firstAttachment in firstMessage.Attachments)
							{
								var attachmentUId = firstAttachment.Id;
								var attachmentName = firstAttachment.Name;
								var attachmentPath = firstAttachment.Path;
								var attachmentPreview = firstAttachment.Preview;
								var attachmentSize = firstAttachment.Size;
								var attachmentType = firstAttachment.Type;
								await _chatService.AddAttachment(addMessage, attachmentUId, attachmentName, attachmentPath, attachmentPreview, attachmentSize, attachmentType);
							}
						}
					}
				}
			}

			return Ok(request);
		}

		[HttpPost]
		[Route("add-message")]
		public async Task<IActionResult> PostMessage([FromBody] PostMessageRequest request)
		{
			var userId = UserId();
			var conversationId = await _chatService.GetConversationId(request.ConversationId);
			var firstMessage = request.MessageData;

			if (firstMessage != null)
			{
				var messageUId = firstMessage.Id;
				var body = firstMessage.Body;
				var contentType = firstMessage.ContentType;
				var addMessage = await _chatService.AddMessage(messageUId, userId, conversationId, body, contentType);

				if (addMessage != 0)
				{
					if (firstMessage.Attachments != null)
					{
						foreach (var firstAttachment in firstMessage.Attachments)
						{
							var attachmentUId = firstAttachment.Id;
							var attachmentName = firstAttachment.Name;
							var attachmentPath = firstAttachment.Path;
							var attachmentPreview = firstAttachment.Preview;
							var attachmentSize = firstAttachment.Size;
							var attachmentType = firstAttachment.Type;

							await _chatService.AddAttachment(addMessage, attachmentUId, attachmentName, attachmentPath, attachmentPreview, attachmentSize, attachmentType);
						}
					}
				}
			}

			return Ok(request);
		}

		private int UserId()
		{
			var userIdClaim = HttpContext.User.FindFirst("userId");
			if (userIdClaim == null)
			{
				return 0;
			}

			int userId = int.Parse(userIdClaim.Value);
			return userId;
		}
	}
}
