using System;

using Borealis.Portal.Core.DependencyInjection;

using System.Linq;

using Borealis.Portal.Domain.Configuration;
using Borealis.Portal.Domain.Effects.Options;

using Microsoft.AspNetCore.Hosting.StaticWebAssets;

using MudBlazor.Services;

using NLog;
using NLog.Extensions.Logging;



namespace Borealis.Portal.Web;


public static class Program
{
    public static IConfiguration Configuration { get; private set; }


    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

        // Configure application core
        ConfigureConfiguration(builder.Configuration);
        ConfigureLogging(builder.Logging, builder.Configuration);

        ConfigureServices(builder.Services);

        WebApplication app = builder.Build();

        ConfigureWebApplication(app);

        await app.RunAsync();
    }


    private static void ConfigureWebApplication(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
    }


    private static void ConfigureServices(IServiceCollection services)
    {
        // Configure options
        services.Configure<JavascriptFilePathsOptions>(Configuration.GetSection(JavascriptFilePathsOptions.Name));

        // Adding the core application services.
        services.AddApplicationServices();

        // Add services to the container.
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddMudServices();
    }


    private static void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        // Adding and setting the appsettings.json to the settings.
        IConfiguration config = builder.SetBasePath(Directory.GetCurrentDirectory())
                                       .AddJsonFile("appsettings.json", false, true)
                                       .Build();

        // Adding the persistanece locations.
        // Key : DatabaseLocation | Value : Database.db
        config[ConfigurationKeys.DatabaseSourceLocation] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.db");

        Configuration = config;
    }


    private static void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration)
    {
        // Getting the configuration for NLog.
        LogManager.Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));

        // Setting NLog as logging provider.
        builder.ClearProviders();
        builder.AddNLog();
    }
}