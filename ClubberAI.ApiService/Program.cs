using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Xml;
using ClubberAI.ServiceDefaults.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.AddMongoDBClient("mongodb");

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddScoped<PartyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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
app.MapGet("/blammo", (PartyService testService) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
	    {
		    var firstBlammo = testService.GetFirst();
		    return new WeatherForecast
		    (
			    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
			    Random.Shared.Next(-20, 55),
			    firstBlammo.Blammo
		    );
	    })
        .ToArray();
    return forecast;
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


