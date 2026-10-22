var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ShopFloorAssistant_Web>("web")
    .WithExternalHttpEndpoints();

builder.Build().Run();
