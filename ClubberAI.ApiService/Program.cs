using ClubberAI.ServiceDefaults;
using ClubberAI.ServiceDefaults.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.AddMongoDBClient("mongodb");

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<PartyService>();
builder.Services.AddSingleton<MusicService>();
builder.Services.AddSingleton<AiProxy>();
builder.AddRedisClient(connectionName: "cache");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/getTodaysParties", (PartyService partyService) => partyService.GetParties());

app.MapGet("/getParticipation", async (HttpContext httpContext, PartyService partyService, [FromQuery] string partyId) =>
{
	var userId = httpContext.User.Identity?.Name;
	if (string.IsNullOrEmpty(userId))
	{
		return Results.Unauthorized();
	}

	var participant = await partyService.GetParticipation(partyId, userId);
	if (participant == null)
	{
		return Results.NotFound();
	}

	return Results.Ok(participant);
});

app.MapGet("/weatherforecast", () =>
{
	var forecast = Enumerable.Range(1, 5).Select(index =>
			new WeatherForecast
			(
				DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				Random.Shared.Next(-20, 55),
				summaries[Random.Shared.Next(summaries.Length)]
			))
		.ToArray();
	return forecast;
});

app.MapGet("/api/photos/{id}", async (string id, PartyService partyService) =>
{
	var photoBlob = await partyService.GetPhoto(id);
	if (photoBlob == null)
	{
		return Results.NotFound();
	}

	return Results.File(photoBlob.Data, photoBlob.ContentType);
});

app.MapGet("/generateParty", async (PartyService partyService) =>
{
    try
    {
        var createdParties = await partyService.CreateParties();
        return Results.Ok(createdParties);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/addParticipant", async (PartyService partyService, [FromQuery] string partyId, [FromQuery] int count = 1) =>
{
	try
	{
		var participants = await partyService.AddParticipants(partyId, count);
		return Results.Ok(participants);
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { message = ex.Message });
	}
});

app.MapGet("/regeneratePhotos", async (PartyService partyService) =>
{
    try
    {
        await partyService.RegenerateAllPhotos();
        return Results.Ok(new { message = "Photos regenerated successfully" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
