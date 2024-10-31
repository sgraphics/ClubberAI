var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
	.WithDataVolume()
	.WithPersistence(TimeSpan.FromSeconds(30), 1);

var apiService = builder
	.AddProject<Projects.ClubberAI_ApiService>("apiservice")
	.WithReference(cache);

builder.AddProject<Projects.ClubberAI_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(cache)
	.WithReference(apiService);

builder.Build().Run();
