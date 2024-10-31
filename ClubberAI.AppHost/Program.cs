var builder = DistributedApplication.CreateBuilder(args);

var openaikey = builder.AddParameter("openaikey", secret: true);
var openaiurl = builder.AddParameter("openaiurl", secret: true);
var mubert = builder.AddParameter("mubert", secret: true);

var cache = builder.AddRedis("cache")
	.WithDataVolume()
	.WithPersistence(TimeSpan.FromSeconds(30), 1);

var apiService = builder
	.AddProject<Projects.ClubberAI_ApiService>("apiservice")
	.WithEnvironment("OpenAiUrl", openaiurl)
	.WithEnvironment("OpenAiKey", openaikey)
	.WithReference(cache);

var apiurl = builder.AddParameter("apiurl", secret: true);

builder.AddProject<Projects.ClubberAI_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(cache)
	.WithReference(apiService)
	.WithEnvironment("OpenAiUrl", openaiurl)
	.WithEnvironment("OpenAiKey", openaikey)
	.WithEnvironment("ApiUrl", apiurl)
	.WithEnvironment("Mubert", mubert);

builder.Build().Run();
