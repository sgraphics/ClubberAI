using Amazon.Runtime.Internal;
using ClubberAI.ServiceDefaults.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Text.Json;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;
using SkiaSharp;

namespace ClubberAI.ServiceDefaults.Services
{
	public class PartyService
	{
		private readonly ILogger<PartyService> _logger;
        private readonly MusicService _musicService;
        private readonly AiProxy _aiProxy;
		private readonly IMongoCollection<Party> _partyCollection;
		private readonly IMongoCollection<Participant> _participantCollection;
		private readonly IMongoCollection<PhotoBlob> _photoCollection;

		public PartyService(IConfiguration config, AiProxy aiProxy, ILogger<PartyService> logger, MusicService musicService)
		{
			_logger = logger;
            _musicService = musicService;
            _aiProxy = aiProxy;
			// Access the connection string from the ConnectionStrings section
			var connectionString = config.GetConnectionString("mongodb");
			var client = new MongoClient(connectionString);

			// Extract database name if included in the connection string
			var database = client.GetDatabase(new MongoUrl(connectionString).DatabaseName);

			_partyCollection = database.GetCollection<Party>("parties");
			_participantCollection = database.GetCollection<Participant>("participants");
			_photoCollection = database.GetCollection<PhotoBlob>("photos");
		}

		public Party GetFirst()
		{
			return _partyCollection.FindSync(Builders<Party>.Filter.Empty).FirstOrDefault();
		}

		public async Task<Participant> AddParticipant(string partyId)
		{
			var party = await _partyCollection.FindSync<Party>(x => x.Id == partyId).FirstOrDefaultAsync();

			var aiMessageRecords = new List<AiMessageRecord>
					{
						new(AiMessageRole.System, @"GPT that generates a party participant based on given description of a party. The participant should be different than those already registered as participants in the party description. Ration of men and women should be balanced. The participant should be of legal age 19-27. The result should be in JSON format.  For example { ""name"" : ""Jill R"", ""gender"" : ""F"", ""age"":""25"", ""photoPrompt"" : ""<FULL PROMPT HERE>"", ""chattingStyle"" :""proper writing, well articulated""  }. ""chattingStyle"" should be a brief description of chatting style. For example ""no capital letters, lots of slang like 'how u doin'"" or ""mostly proper, but makes a few spelling mistakes 'Hi, how are you dnoing?'"". The ""photoPrompt"" is a description for DALL-E to create a photo of that specific participants at the specific party: the background, clothing, vibe etc should reflect the party atmosphere, there ofcourse should be other people, partygoers, in the background and the participant should be sexy, enticing, flirty, handsom/cute. The ""photoPrompt"" can optionally detail the participant doing something like holding drink, dancing, showing some had sign, etc."),
						new(AiMessageRole.User, JsonSerializer.Serialize(party))
					};

			var result = await _aiProxy.Think(aiMessageRecords);
			var participant = JsonSerializer.Deserialize<Participant>(result, JsonOptions.DefaultOptions)!;

			participant.PartyId = party.Id;

			party.Participants!.Add(participant);

			await GeneratePhotos(participant);

			var update = Builders<Party>.Update
				.Set(p => p.Participants, party.Participants);

			await _partyCollection.UpdateOneAsync(x => x.Id == partyId, update);

			return participant;
		}

		public async Task<IList<Party>> GetParties()
		{
			var date = DateTimeOffset.UtcNow.ToString("yy-MM-dd"); // "24-10-23"

			var parties = await _partyCollection.FindSync(x => x.Date == date).ToListAsync();
			foreach (var party in parties)
			{
				var participants = await _participantCollection.FindSync<Participant>(x => x.PartyId == party.Id).ToListAsync();
				party.ParticipantCount = participants.Count;
				party.SomePhotos = participants
					.OrderBy(x => Guid.NewGuid()).Take(3).Select(x => x.PhotoThumb!)
					.ToList();
				party.Participants = null;
			}
			return parties;
		}

		public async Task<Participant> GetParticipation(string partyId, string userId)
		{
			return await _participantCollection
				.FindSync<Participant>(x => x.PartyId == partyId && x.User == userId)
				.FirstOrDefaultAsync();
		}

		public async Task<ActionResult> CreateParties()
		{
			var date = DateTimeOffset.Now
                //.AddDays(1)
                .ToString("yy-MM-dd");
			var parties = new List<Party>();
			var participants = new List<Participant>();

			parties = await _partyCollection.FindSync<Party>(x => x.Date == date).ToListAsync();
			var partiesToInsert = new List<Party>();
			var channels = await _musicService.GetChannelsForAi();

            for (var i = 0; i < 1; i++)
			{
				var aiMessageRecords = new List<AiMessageRecord>
				{
					new(AiMessageRole.System, @"GPT that generates party descriptions. these are parties happening all around the city in different places. All parties are 18+ and happen in the evening (end time based on party). Parties should be interesting, some over the top, sexy, promising good time and chance to meet someone special. They can range from glamorous and expensive to student parties in abandoned dormitories.

The answer need to be in json array format (to support multiple parties) with these required properties:
1) partyName
2) musicStyle
3) location: free description
4) dressCode: can be  ""people usually come in"" or ""people usually wear"" or something specific ""business casual""
5) type: is it a party ""series"" or ""oneTime"" happening
6) participants: list of participants and their description. The participant should be of legal age 19-27.  For example ""participants"" : [ { ""name"" : ""Jill R"", ""gender"" : ""F"", ""age"":""25"", ""photoPrompt"" : ""<FULL PROMPT HERE>"", ""chattingStyle"" :""proper writing, well articulated""  } ]. ""chattingStyle"" should be a brief description of chatting style. For example ""no capital letters, lots of slang like 'how u doin'"" or ""mostly proper, but makes a few spelling mistakes 'Hi, how are you dnoing?'"". The ""photoPrompt"" is a description for DALL-E to create a photo of that specific participants at the specific party: the background, clothing, vibe etc should reflect the party atmosphere, there ofcourse should be other people, partygoers, in the background and the participant should be sexy, enticing, flirty, handsom/cute. The ""photoPrompt"" can optionally detail the participant doing something like holding drink, dancing, showing some had sign, etc.
7) primaryColor: in hex format, something like #123AAB (similar to theme of the image)
8) description: 350 character description about the party. It should detail the premise, what will happen, special guests, special events etc. It should feature some over the top performance that makes it sound like it will be the best party ever.
9) flyerPrompt: input prompt to generate image of the party flyer using DALL-E. The flyer is an image of the party. It takes input details about an event or party, including theme, vibe, music style, and target audience to produce a textual description that can be used by DALL-E to create an image. There should be empty space in it for writing additional info - large area for heading. There should be one word on the flyer that describes the event or is a word from the title of the event. The image should be simple and impactful. It should be minimalistic and tasteful with one or two large objects as focal point. Objects can be abstract illustrations or actual real world objects and scenes. Never ask the human more details, try to design as best as you can with as much information as you have. Do not describe the flyer, let the image do the talking. If needed you can browse the internet for specific information - for example if someone mentiones some fact about the party you do not understand. Do NOT say to Dall-E that we want to create flyers or ads or posters - ALWAYS and ONLY refer to the end result as ""image"" . Do not mention artifacts like flyer, ad, poster, illustration, drawing - try to describe the content differently while only referring to an ""image"".
10) musicChannel: channel id, something like 1.0.1. Please select a suitable channel from these options:
" + channels),
				};
				if (parties.Any())
				{
					var partiesString = string.Join(",", parties.Select(x => $"{x.PartyName} - {x.MusicStyle}"));
					aiMessageRecords.Add(new AiMessageRecord(AiMessageRole.User, $"Create a party with 1 participant. It needs to be different than these: {partiesString}"));
				}
				else
				{
					aiMessageRecords.Add(new AiMessageRecord(AiMessageRole.User, "Create a party with 1 participant."));
				}
				var result = await _aiProxy.Think(aiMessageRecords);
				var newParties = JsonSerializer.Deserialize<IList<Party>>(result, JsonOptions.DefaultOptions);
				foreach (var newParty in newParties!)
				{
					foreach (var participant in newParty.Participants!)
					{
						participant.PartyId = newParty.Id;
						participants.Add(participant);
					}

					newParty.Date = date;
				}
				partiesToInsert.AddRange(newParties);
				parties.AddRange(newParties);
			}

			foreach (var party in partiesToInsert)
			{
				await CreatePhotos(party);
				await _partyCollection.InsertOneAsync(party);

				foreach (var participant in participants.Where(x => x.PartyId == party.Id))
				{
					await GeneratePhotos(participant);
				}
			}

			return new JsonResult(new
			{
				parties,
				participants
			});
		}


		public async Task GeneratePhotos(Participant participant, bool store = true)
		{
			string participantDescription;
			if (!string.IsNullOrWhiteSpace(participant.Description))
			{
				participantDescription = participant.Description;
			}
			else
			{
				var partition = DateTimeOffset.UtcNow.ToString("yy-MM-dd");
				var party = (await _partyCollection.FindSync<Party>(x => x.Date == partition && x.Id == participant.PartyId).FirstOrDefaultAsync());
				var prompt = @$"The photo background, clothing, vibe etc should reflect the party atmosphere, there ofcourse should be other people, partygoers, in the background and the participant should be sexy, enticing, flirty, handsom/cute. The participant should be of age {participant.Age}. The prompt can optionally detail the participant doing something like holding drink, dancing, showing some had sign, etc. The participant characteristics: hairstyle: {participant.HairStyle}, gender: {participant.Gender}, ethnicity: {participant.Ethnicity}. This is the party description: " + party.Description + ". Music style: " + party.MusicStyle + ". Dresscode: " + party.DressCode;
				participantDescription = await _aiProxy.Think(new List<AiMessageRecord>
				{
					new (AiMessageRole.System, "Generate a prompt for DALL-E to create a photo of a specific participant at a specific party. IMPORTANT: you must only answer with the prompt, not other text is allowed."),
					new(AiMessageRole.User, prompt)
				});
			}
			var image = await _aiProxy.CreateImage(participantDescription);
			var photo = await UploadPhoto(image);
			participant.Photo = photo.Url;
			participant.PhotoThumb = photo.ThumbUrl;
			if (store)
			{
				await _participantCollection.ReplaceOneAsync(x => x.Id == participant.Id, participant, new ReplaceOptions { IsUpsert = true });
			}
		}

		private async Task CreatePhotos(Party participant)
		{
			var image = await _aiProxy.CreateImage(participant.FlyerPrompt);
			var photo = await UploadPhoto(image);
			participant.Photo = photo.Url;
			participant.PhotoThumb = photo.ThumbUrl;
		}

		public async Task<PhotoBlob> GetPhoto(string id)
		{
			return await _photoCollection.FindSync(x => x.Id == id).FirstOrDefaultAsync();
		}
		public async Task<Photo> UploadPhoto(string base64Image)
		{
			try
			{
				var data = Convert.FromBase64String(base64Image);

				// Store original image
				var originalImage = new PhotoBlob
				{
					Data = data,
					ContentType = "image/png",
					CreatedAt = DateTime.UtcNow
				};

				await _photoCollection.InsertOneAsync(originalImage);

				// Generate thumbnail
				using (var stream = new MemoryStream(data))
				{
					var thumbnail = GetReducedImage(200, stream);
					byte[] thumbnailData;
					using (var ms = new MemoryStream())
					{
						thumbnail.Encode(ms, SKEncodedImageFormat.Png, 100);
						thumbnailData = ms.ToArray();
					}

					var thumbnailImage = new PhotoBlob
					{
						Data = thumbnailData,
						ContentType = "image/png",
						CreatedAt = DateTime.UtcNow,
						OriginalImageId = originalImage.Id
					};

					await _photoCollection.InsertOneAsync(thumbnailImage);

					return new Photo
					{
						Url = $"api/photos/{originalImage.Id}",
						ThumbUrl = $"api/photos/{thumbnailImage.Id}"
					};
				}
			}
			catch (Exception e)
			{
				_logger?.LogError(e, "Image upload failed: " + e.Message);
				throw;
			}
		}

		private SKBitmap GetReducedImage(int maxWidth, Stream resourceImage)
		{
			try
			{
				using (var original = SKBitmap.Decode(resourceImage))
				{
					float aspectRatio = (float)original.Width / original.Height;
					int newWidth = maxWidth;
					int newHeight = (int)(newWidth / aspectRatio);

					var imageInfo = new SKImageInfo(newWidth, newHeight);
					var resized = original.Resize(imageInfo, SKFilterQuality.High);
					return resized;
				}
			}
			catch (Exception e)
			{
				_logger?.LogError(e, "Thumbnail generation failed: " + e.Message);
				return null;
			}
		}

		public async Task Save(Participant newParticipant)
		{
			await _participantCollection.InsertOneAsync(newParticipant);
		}
	}
}
