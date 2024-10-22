using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;

namespace ClubberAI.ServiceDefaults.Model
{
	public class EntityBase
	{
		[BsonId] // Specifies that this is the primary key field.
		[BsonRepresentation(BsonType.ObjectId)] // Maps the ObjectId from MongoDB to a string in .NET.
		public string Id { get; set; }
	}

	public class Party : EntityBase
	{
		[BsonElement("blammo")] // Maps the "Name" property to the "Name" field in the MongoDB document.
		public string Blammo { get; set; }

		[BsonElement("participants")]
		public IList<Participant>? Participants { get; set; } = new List<Participant>();

		[BsonElement("partyName")]
		public string PartyName { get; init; }

		[BsonElement("musicStyle")]
		public string MusicStyle { get; init; }

		[BsonElement("location")]
		public string Location { get; init; }

		[BsonElement("dressCode")]
		public string DressCode { get; init; }

		[BsonElement("type")]
		public string Type { get; init; }

		[BsonElement("primaryColor")]
		public string PrimaryColor { get; init; }

		[BsonElement("description")]
		public string Description { get; init; }

		[BsonElement("flyerPrompt")]
		public string FlyerPrompt { get; init; }

		[BsonElement("photo")]
		public string? Photo { get; set; }

		[BsonElement("photoThumb")]
		public string? PhotoThumb { get; set; }

		[BsonIgnore]
		public int ParticipantCount { get; set; }

		[BsonIgnore]
		public List<string> SomePhotos { get; set; } = new();
	}

	public class Photo : EntityBase
	{
		[BsonElement("url")]
		public string Url { get; set; }

		[BsonElement("thumbUrl")]
		public string ThumbUrl { get; set; }
	}

	public class User : EntityBase
	{
		[BsonElement("gender")]
		public string Gender { get; set; }
	}

	public class Participant : EntityBase
	{
		[BsonElement("photo")]
		public string? Photo { get; set; }

		[BsonElement("photoThumb")]
		public string? PhotoThumb { get; set; }

		[BsonElement("hairStyle")]
		public string? HairStyle { get; set; }

		[BsonElement("ethnicity")]
		public string? Ethnicity { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("gender")]
		public string Gender { get; set; }

		[BsonElement("age")]
		public string Age { get; set; }

		[BsonElement("photoPrompt")]
		public string? Description { get; set; }

		[BsonElement("chattingStyle")]
		public string? ChattingStyle { get; set; }

		[BsonElement("user")]
		public string? User { get; set; }

		[BsonElement("activeChatId")]
		public string? ActiveChatId { get; set; }

		[BsonElement("score")]
		public int Score { get; set; }
	}

	public class ChatData : EntityBase
	{
		[BsonElement("participant1Id")]
		public string Participant1Id { get; set; }

		[BsonElement("participant1Name")]
		public string Participant1Name { get; set; }

		[BsonElement("participant1Age")]
		public string Participant1Age { get; set; }

		[BsonElement("participant2Id")]
		public string? Participant2Id { get; set; }

		[BsonElement("participant2Name")]
		public string? Participant2Name { get; set; }

		[BsonElement("participant2Age")]
		public string? Participant2Age { get; set; }

		[BsonIgnore]
		public Party Party { get; set; }

		[BsonIgnore]
		public Participant Participant { get; set; }

		[BsonIgnore]
		public Participant? OtherParticipant => string.IsNullOrWhiteSpace(Participant2Id)
			? null
			: new()
			{
				Name = Participant1Id == Participant.Id ? Participant2Name : Participant1Name,
				Id = Participant1Id == Participant.Id ? Participant2Id : Participant1Id,
				Age = Participant1Id == Participant.Id ? Participant2Age : Participant1Age,
				PhotoThumb = Participant1Id == Participant.Id ? Participant2PhotoThumb : Participant1PhotoThumb,
				Photo = Participant1Id == Participant.Id ? Participant2Photo : Participant1Photo,
			};

		[BsonIgnore]
		public List<ChatMessageData> History { get; set; } = new();

		[BsonElement("historyJson")]
		public string HistoryJson
		{
			get => JsonSerializer.Serialize(History);
			set => History = JsonSerializer.Deserialize<List<ChatMessageData>>(value);
		}

		[BsonElement("participant1PhotoThumb")]
		public string? Participant1PhotoThumb { get; set; }

		[BsonElement("participant2PhotoThumb")]
		public string? Participant2PhotoThumb { get; set; }

		[BsonElement("participant1Photo")]
		public string? Participant1Photo { get; set; }

		[BsonElement("participant2Photo")]
		public string? Participant2Photo { get; set; }
	}

	public class ChatMessageData
	{
		[BsonElement("participantId")]
		public string ParticipantId { get; set; }

		[BsonElement("timestamp")]
		public DateTimeOffset Timestamp { get; set; }

		[BsonElement("message")]
		public string? Message { get; set; }

		[BsonElement("isBot")]
		public bool? IsBot { get; set; }
	}
}
