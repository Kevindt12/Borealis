using System.Net;

using Borealis.Portal.Rpi.Contexts;
using Borealis.Portal.Rpi.Services;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using NLog;
using NLog.Extensions.Logging;



var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
ConfigureConfiguration(builder.Configuration);
ConfigureLogging(builder.Logging, builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddSingleton<LedstripContext>();

builder.Services.AddHostedService<DriverHostedService>();

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.Listen(IPAddress.Any,
                         5004,
                         options => { options.Protocols = HttpProtocols.Http2; });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CoreService>();

//app.MapGrpcService<LedstripConfigurationService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

static void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration)
{
    LogManager.Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));

    builder.ClearProviders();
    builder.AddNLog();
}

static void ConfigureConfiguration(ConfigurationManager builder)
{
    IConfiguration config = builder.SetBasePath(Directory.GetCurrentDirectory())
                                   .AddJsonFile("appsettings.json", false, true)
                                   .Build();
}