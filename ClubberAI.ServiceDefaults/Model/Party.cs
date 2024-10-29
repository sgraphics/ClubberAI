using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

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
		[BsonElement("date")]
		[JsonPropertyName("date")]
		public string Date { get; set; }

		[BsonElement("participants")]
		[JsonPropertyName("participants")]
		public IList<Participant>? Participants { get; set; } = new List<Participant>();

		[BsonElement("partyName")]
		[JsonPropertyName("partyName")]
		public string PartyName { get; init; }

		[BsonElement("musicStyle")]
		[JsonPropertyName("musicStyle")]
		public string MusicStyle { get; init; }

		[BsonElement("location")]
		[JsonPropertyName("location")]
		public string Location { get; init; }

		[BsonElement("dressCode")]
		[JsonPropertyName("dressCode")]
		public string DressCode { get; init; }

		[BsonElement("type")]
		[JsonPropertyName("type")]
		public string Type { get; init; }

		[BsonElement("primaryColor")]
		[JsonPropertyName("primaryColor")]
		public string PrimaryColor { get; init; }

		[BsonElement("description")]
		[JsonPropertyName("description")]
		public string Description { get; init; }

		[BsonElement("flyerPrompt")]
		[JsonPropertyName("flyerPrompt")]
		public string FlyerPrompt { get; init; }

		[BsonElement("photo")]
		[JsonPropertyName("photo")]
		public string? Photo { get; set; }

		[BsonElement("photoThumb")]
		[JsonPropertyName("photoThumb")]
		public string? PhotoThumb { get; set; }

		[BsonIgnore]
		public int ParticipantCount { get; set; }

		[BsonIgnore]
		public List<string> SomePhotos { get; set; } = new();

		[BsonElement("musicChannel")]
		[JsonPropertyName("musicChannel")]
		public string? MusicChannel { get; set; }
	}

	public class Photo : EntityBase
	{
		[BsonElement("url")]
		[JsonPropertyName("url")]
		public string Url { get; set; }

		[BsonElement("thumbUrl")]
		[JsonPropertyName("thumbUrl")]
		public string ThumbUrl { get; set; }
	}

	public class PhotoBlob : EntityBase
	{
		[BsonElement("data")]
		[JsonPropertyName("data")]
		public byte[] Data { get; set; }

		[BsonElement("contentType")]
		[JsonPropertyName("contentType")]
		public string ContentType { get; set; }

		[BsonElement("createdAt")]
		[JsonPropertyName("createdAt")]
		[BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Ensures the DateTime is stored in UTC
		public DateTime CreatedAt { get; set; }

		[BsonElement("originalImageId")]
		[JsonPropertyName("originalImageId")]
		public string OriginalImageId { get; set; }
	}

	public class User : EntityBase
	{
		[BsonElement("gender")]
		[JsonPropertyName("gender")]
		public string Gender { get; set; }
	}

	public class Participant : EntityBase
	{
		[BsonElement("partyId")]
		[JsonPropertyName("partyId")]
		public string? PartyId { get; set; }

		[BsonElement("photo")]
		[JsonPropertyName("photo")]
		public string? Photo { get; set; }

		[BsonElement("photoThumb")]
		[JsonPropertyName("photoThumb")]
		public string? PhotoThumb { get; set; }

		[BsonElement("hairStyle")]
		[JsonPropertyName("hairStyle")]
		public string? HairStyle { get; set; }

		[BsonElement("ethnicity")]
		[JsonPropertyName("ethnicity")]
		public string? Ethnicity { get; set; }

		[BsonElement("name")]
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[BsonElement("gender")]
		[JsonPropertyName("gender")]
		public string Gender { get; set; }

		[BsonElement("age")]
		[JsonPropertyName("age")]
		public string Age { get; set; }

		[BsonElement("photoPrompt")]
		[JsonPropertyName("photoPrompt")]
		public string? Description { get; set; }

		[BsonElement("chattingStyle")]
		[JsonPropertyName("chattingStyle")]
		public string? ChattingStyle { get; set; }

		[BsonElement("user")]
		[JsonPropertyName("user")]
		public string? User { get; set; }

		[BsonElement("activeChatId")]
		[JsonPropertyName("activeChatId")]
		public string? ActiveChatId { get; set; }

		[BsonElement("score")]
		[JsonPropertyName("score")]
		public int Score { get; set; }
	}

	public class ChatData : EntityBase
	{
		[BsonElement("participant1Id")]
		[JsonPropertyName("participant1Id")]
		public string Participant1Id { get; set; }

		[BsonElement("participant1Name")]
		[JsonPropertyName("participant1Name")]
		public string Participant1Name { get; set; }

		[BsonElement("participant1Age")]
		[JsonPropertyName("participant1Age")]
		public string Participant1Age { get; set; }

		[BsonElement("participant2Id")]
		[JsonPropertyName("participant2Id")]
		public string? Participant2Id { get; set; }

		[BsonElement("participant2Name")]
		[JsonPropertyName("participant2Name")]
		public string? Participant2Name { get; set; }

		[BsonElement("participant2Age")]
		[JsonPropertyName("participant2Age")]
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
		[JsonPropertyName("historyJson")]
		public string HistoryJson
		{
			get => JsonSerializer.Serialize(History);
			set => History = JsonSerializer.Deserialize<List<ChatMessageData>>(value, JsonOptions.DefaultOptions);
		}

		[BsonElement("participant1PhotoThumb")]
		[JsonPropertyName("participant1PhotoThumb")]
		public string? Participant1PhotoThumb { get; set; }

		[BsonElement("participant2PhotoThumb")]
		[JsonPropertyName("participant2PhotoThumb")]
		public string? Participant2PhotoThumb { get; set; }

		[BsonElement("participant1Photo")]
		[JsonPropertyName("participant1Photo")]
		public string? Participant1Photo { get; set; }

		[BsonElement("participant2Photo")]
		[JsonPropertyName("participant2Photo")]
		public string? Participant2Photo { get; set; }
	}

	public class ChatMessageData
	{
		[BsonElement("participantId")]
		[JsonPropertyName("participantId")]
		public string ParticipantId { get; set; }

		[BsonElement("timestamp")]
		[JsonPropertyName("timestamp")]
		public DateTimeOffset Timestamp { get; set; }

		[BsonElement("message")]
		[JsonPropertyName("message")]
		public string? Message { get; set; }

		[BsonElement("isBot")]
		[JsonPropertyName("isBot")]
		public bool? IsBot { get; set; }
	}
}
