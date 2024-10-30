using System.Diagnostics;
using System.Reactive.Subjects;
using ClubberAI.ServiceDefaults.Model;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace ClubberAI.ServiceDefaults.Services
{
	public class ChatService
	{
		private readonly IConfiguration _configuration;
		private readonly AiProxy _aiProxy;
		private readonly ILogger<ChatService> _logger;
		private readonly IMongoCollection<Party> _partiesCollection;
		private readonly IMongoCollection<Participant> _participantsCollection;
		private readonly IMongoCollection<ChatData> _chatsCollection;
		private readonly IMongoCollection<User> _userCollection;

		public ChatService(IConfiguration configuration, AiProxy aiProxy, ILogger<ChatService> logger)
		{
			_configuration = configuration;
			_aiProxy = aiProxy;
			_logger = logger;
			var connectionString = _configuration.GetConnectionString("mongodb");
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(new MongoUrl(connectionString).DatabaseName);

			_partiesCollection = database.GetCollection<Party>("parties");
			_participantsCollection = database.GetCollection<Participant>("participants");
			_chatsCollection = database.GetCollection<ChatData>("chats");
			_userCollection = database.GetCollection<User>("users");
		}

		[HttpGet]
		[Route("startChat")]
		public async Task<ChatData> StartChat(string partyId, string user)
		{
			var party = await _partiesCollection.FindSync<Party>(x => x.Id == partyId).FirstOrDefaultAsync();

			var participants =
				await _participantsCollection.FindSync<Participant>(x => x.PartyId == party.Id).ToListAsync();
			var participant = participants.OrderByDescending(x => x.Timestamp).First(x => x.User == user);

			ChatData chat;
			if (!string.IsNullOrWhiteSpace(participant.ActiveChatId))
			{
				chat = _chatsCollection.FindSync<ChatData>(x => x.PartyId == partyId && x.Id == participant.ActiveChatId)?.FirstOrDefault();
			}
			else
			{
				var previousChats = await _chatsCollection.FindSync<ChatData>(x => x.PartyId == partyId && (x.Participant1Id == participant.Id || x.Participant2Id == participant.Id))
					.ToListAsync();

				var previousPartners = previousChats.SelectMany(x => new[] { x.Participant1Id, x.Participant2Id })
					.ToHashSet();

				var participant2 = participants
					.Where(x => string.IsNullOrWhiteSpace(x.ActiveChatId) && x.Gender != participant.Gender && x.User != user)
					.Where(x => !previousPartners.Contains(x.Id))
					.MinBy(x => Guid.NewGuid());

				chat = new ChatData
				{
					Participant1Id = participant.Id,
					Participant1Name = participant.Name,
					Participant1Age = participant.Age,
					Participant1PhotoThumb = participant.PhotoThumb,
					Participant2Id = participant2?.Id,
					Participant2Age = participant2?.Age,
					Participant2Name = participant2?.Name,
					Participant2PhotoThumb = participant2?.PhotoThumb,
					PartyId = partyId,
				};
				if (participant2 != null)
				{
					await _chatsCollection.InsertOneAsync(chat);
					await _participantsCollection.UpdateOneAsync(
						x => x.Id == participant2.Id,
						Builders<Participant>.Update.Set(x => x.ActiveChatId, chat.Id)
					);
					await _participantsCollection.UpdateOneAsync(
						x => x.Id == participant.Id,
						Builders<Participant>.Update.Set(x => x.ActiveChatId, chat.Id)
					);
				}
			}

			chat.Party = party;
			chat.Participant = participant;

			return chat;
		}

		public async Task Say(string newChat, ChatData chat)
		{
			var participants = await _participantsCollection.FindSync<Participant>(x => x.ActiveChatId == chat.Id).ToListAsync();

			var participant = chat.Participant;
			var otherParticipant = participants.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.User) || x.User != chat.Participant.User);

			var chatMessageData = new ChatMessageData { Message = newChat, Timestamp = DateTimeOffset.UtcNow, ParticipantId = participant.Id };
			chat.History.Add(chatMessageData);
			await _chatsCollection.ReplaceOneAsync(x => x.Id == chat.Id, chat, new ReplaceOptions { IsUpsert = true });

			ChatUpdates.OnNext((chat.Id, chatMessageData));

			if (string.IsNullOrWhiteSpace(otherParticipant!.User))
			{
				//is BOT
				Task.Run(async () =>
				{
					Stopwatch stopper = new Stopwatch();
					stopper.Start();

					var aiMessageRecords = new List<AiMessageRecord>
					{
						new(AiMessageRole.System, @$"Conversation is between the user ({participant.Name}, {participant.Gender}, {participant.Age}) and a person called {otherParticipant.Name}. {otherParticipant.Name} is a {otherParticipant.Age} year old {otherParticipant.Gender}, who looks like this: {otherParticipant.Description}. {otherParticipant.Name} chatting style is as follows: {otherParticipant.ChattingStyle}. All generated answers must follow this writing style, and overall feel like a real person is writing. IMPORTANT: {otherParticipant.Name} message should be VERY short, brief, MAXIMUM 2 sentences, but can be even shorter like only 3 words. {otherParticipant.Name} aim is to answer questions but also ask questions about the user to learn more and perhaps hook up. Conversation can be flirty. Conversation takes place at a party: {chat.Party}, dress code: {chat.Party.DressCode}, music style {chat.Party.MusicStyle}.")
					};

					AddChatHistory(chat, aiMessageRecords);

					aiMessageRecords.Add(new(AiMessageRole.User, @$"{newChat}"));

					var writeOut = await _aiProxy.Think(aiMessageRecords);

					var aiChatMessage = new ChatMessageData { Message = writeOut, Timestamp = DateTimeOffset.UtcNow, ParticipantId = otherParticipant.Id };

					chat.History.Add(aiChatMessage);

					await _chatsCollection.ReplaceOneAsync(x => x.Id == chat.Id, chat, new ReplaceOptions { IsUpsert = true });

					stopper.Stop();

					var seconds = stopper.ElapsedMilliseconds / 1000;

					var shouldTakeSeconds = writeOut.Length / 3;

					if (shouldTakeSeconds > seconds)
					{
						await Task.Delay(TimeSpan.FromSeconds(shouldTakeSeconds - seconds));
					}

					ChatUpdates.OnNext((chat.Id, aiChatMessage));
				});
			}
		}

		/// <summary>
		/// chat id
		/// </summary>
		public static readonly Subject<(string, ChatMessageData)>
			ChatUpdates = new Subject<(string, ChatMessageData)>();

		void AddChatHistory(ChatData chat, List<AiMessageRecord> aiMessageRecords)
		{
			foreach (var message in chat.History.OrderBy(x => x.Timestamp))
			{
				aiMessageRecords.Add(new AiMessageRecord(message.ParticipantId == chat.Participant.Id ? AiMessageRole.User : AiMessageRole.Assistant, message.Message));
			}
		}

		public async Task<bool> EndChat(ChatData chat, bool isBot, string userId)
		{
			var user = await _userCollection.FindSync<User>(x => x.Id == userId).FirstOrDefaultAsync();
			var currentScore = user?.Score ?? 0;

			var participants = await _participantsCollection.FindSync<Participant>(x => x.ActiveChatId == chat.Id).ToListAsync();

			var participant = chat.Participant;
			var otherParticipant = participants.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.User) || x.User != chat.Participant.User);
			bool isScore = false;
			if (string.IsNullOrWhiteSpace(otherParticipant.User))
			{
				//is bot
				if (isBot)
				{
					await _userCollection.UpdateOneAsync(
						x => x.Id == userId,
						Builders<User>.Update.Set(x => x.Score, ++currentScore),
						new UpdateOptions { IsUpsert = true }
					);
					isScore = true;
				}
			}
			else
			{
				if (!isBot)
				{
					await _userCollection.UpdateOneAsync(
						x => x.Id == userId,
						Builders<User>.Update.Set(x => x.Score, ++currentScore),
						new UpdateOptions { IsUpsert = true }
					);
					isScore = true;
				}
			}
			await ResetActiveChat(participant, otherParticipant);

			ChatUpdates.OnNext((chat.Id, new ChatMessageData
			{
				ParticipantId = chat.Participant.Id,
				Timestamp = DateTimeOffset.UtcNow,
				IsBot = isBot
			}));

			return isScore;
		}

		private async Task ResetActiveChat(Participant participant,
			Participant otherParticipant)
		{
			await _participantsCollection.UpdateOneAsync(
				x => x.Id == participant.Id,
				Builders<Participant>.Update.Set(x => x.ActiveChatId, "")
			);

			await _participantsCollection.UpdateOneAsync(
				x => x.Id == otherParticipant.Id,
				Builders<Participant>.Update.Set(x => x.ActiveChatId, "")
			);
		}
	}
}
