using GrpcMultitenantService.Infrastructure.CommonEntityModels.Interfaces;
using GrpcMultitenantService.Infrastructure.CommonEntityModels;
using GrpcMultitenantService.Common;
using GrpcMultitenantService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using GrpcMultitenantService.Features.Tracking.Services;
using GrpcMultitenantService.Features.ProviderClaim.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.WebHost.ConfigureKestrel(options =>
{
    // gRPC endpoint (HTTP/2)
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    // REST endpoint (HTTP/1.1)
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<MultitenancyInterceptor>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProviderClaimService>();
app.MapGrpcService<TrackingService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGet("/hello", () => Results.Ok("Hello, world!"));

app.Run();
