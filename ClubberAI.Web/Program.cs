using ClubberAI.ServiceDefaults.Services;
using ClubberAI.Web;
using Blazr.RenderState.Server;
using ClubberAI.Web.Components;
using AzureMapsControl.Components;
using ClubberAI.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.Services.AddSingleton<PartyService>();
builder.Services.AddSingleton<AiProxy>();
builder.Services.AddSingleton<BalanceService>();
builder.Services.AddScoped<NearWalletService>();
builder.Services.AddScoped<AudioPlayerService>();
builder.Services.AddSingleton<MusicService>();
builder.AddRedisOutputCache("cache");
builder.Services.AddServiceDiscovery();


builder.Services.AddBootstrapBlazor();
builder.AddBlazrRenderStateServerServices();
builder.Services.AddAzureMapsControl(
	configuration =>
	{
		var key = "9us7lWqyKd-0QjY9WKXcvgx6uxKh63NKq8vCYcOfQOA";
		configuration.SubscriptionKey = key;
	});


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });
builder.Services.AddHttpClient<PartyApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
