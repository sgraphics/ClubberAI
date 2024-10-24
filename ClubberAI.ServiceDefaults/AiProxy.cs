using System.ClientModel;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Console = System.Console;

namespace ClubberAI.ServiceDefaults
{
	public class AiProxy
	{
		private readonly IConfiguration _configuration;
		public AiProxy(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<string> Think(IList<AiMessageRecord> messages)
		{
			var streamUpdates = ThinkStream(messages, out var completionRequest);
			var resultBuilder = new StringBuilder();
			// Iterate over the streaming updates
			await foreach (var update in streamUpdates)
			{
				// Append the content updates
				foreach (var contentUpdate in update.ContentUpdate)
				{
					resultBuilder.Append(contentUpdate.Text);
				}
			}

			// Return the concatenated result
			return resultBuilder.Replace("```json", string.Empty).Replace("```", string.Empty).ToString();
		}

		private AsyncCollectionResult<StreamingChatCompletionUpdate> ThinkStream(IList<AiMessageRecord> messages, out ChatCompletionOptions completionRequest)
		{
			var apiKey = _configuration["OpenAiKey"];
			var apiUrl = _configuration["OpenAiUrl"];

			AzureOpenAIClient api = new(new Uri(apiUrl), new ApiKeyCredential(apiKey));

			completionRequest = new ChatCompletionOptions()
			{
				Temperature = 0.7f,
				MaxOutputTokenCount = 2000,
				FrequencyPenalty = 0.1f,
				PresencePenalty = 0,
			};
			var messagesList = messages
				.Select<AiMessageRecord, ChatMessage>(x =>
				{
					switch (x.Role)
					{
						case AiMessageRole.System:
							return new SystemChatMessage(x.Content);
						case AiMessageRole.User:
							return new UserChatMessage(x.Content);
						case AiMessageRole.Assistant:
							return new AssistantChatMessage(x.Content);
					}

					throw new NotImplementedException();
				})
				.ToList();
			return api.GetChatClient("toolblox-gpt4o").CompleteChatStreamingAsync(messagesList);
		}

		public async Task<string> CreateImage(string prompt)
		{
			var apiKey = _configuration["OpenAiKey"];
			var apiUrl = _configuration["OpenAiUrl"];

			AzureOpenAIClient api = new(new Uri(apiUrl), new ApiKeyCredential(apiKey));

			var imageClient = api.GetImageClient("dall-e-3");
			var imageGenerationRequest = new OpenAI.Images.ImageGenerationOptions
			{
				Size = OpenAI.Images.GeneratedImageSize.W1024xH1792,
				Quality = OpenAI.Images.GeneratedImageQuality.High,
				ResponseFormat = OpenAI.Images.GeneratedImageFormat.Bytes,
				Style = OpenAI.Images.GeneratedImageStyle.Vivid
			};

			var response = await imageClient.GenerateImageAsync(prompt, imageGenerationRequest);

			if (response?.Value?.ImageBytes != null)
			{
				var imageBytes = response.Value.ImageBytes;
				var base64Image = Convert.ToBase64String(imageBytes.ToArray());
				Console.WriteLine($"Image generated. Base64 length: {base64Image.Length}");
				return base64Image;
			}
			else
			{
				Console.WriteLine("No image data received.");
				throw new Exception("No image data received from the API.");
			}
		}

	}

	public record AiMessageRecord(AiMessageRole Role, string Content)
	{
	}

	public enum AiMessageRole
	{
		System,
		Assistant,
		User
	}
}
