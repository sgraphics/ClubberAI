using System.Text.Json;

namespace ClubberAI.ServiceDefaults
{
	public static class JsonOptions
	{
		public static JsonSerializerOptions DefaultOptions => new()
		{
			AllowTrailingCommas = true
		};
	}
}
