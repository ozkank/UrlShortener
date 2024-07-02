var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                 .AddDatabase("sqldata");

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.UrlShortener_ApiService>("apiservice")
                                                     .WithReference(sql);

builder.AddProject<Projects.UrlShortener_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService);

builder.Build().Run();
