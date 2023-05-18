using System;

using Borealis.Portal.Data.DependencyInjection;
using Borealis.Portal.Domain.DependencyInjection;
using Borealis.Portal.Infrastructure.DependencyInjection;

using System.Linq;

using Borealis.Portal.Core.Animations;
using Borealis.Portal.Core.Devices.Contexts;
using Borealis.Portal.Core.Devices.Managers;
using Borealis.Portal.Core.Devices.Services;
using Borealis.Portal.Core.Effects.Factories;
using Borealis.Portal.Core.Effects.Managers;
using Borealis.Portal.Core.Ledstrips.Contexts;
using Borealis.Portal.Core.Ledstrips.Managers;
using Borealis.Portal.Core.Ledstrips.Services;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Devices.Managers;
using Borealis.Portal.Domain.Devices.Services;
using Borealis.Portal.Domain.Effects.Factories;
using Borealis.Portal.Domain.Effects.Managers;
using Borealis.Portal.Domain.Ledstrips.Managers;
using Borealis.Portal.Domain.Ledstrips.Services;

using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Core.DependencyInjection;


public static class CoreServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Dependencies
        services.AddDomainServices();
        services.AddInfrastructureServices();
        services.AddDataServices();

        // Factories
        services.AddTransient<IEffectEngineFactory, EffectEngineFactory>();
        services.AddTransient<IAnimationPlayerFactory, AnimationPlayerFactory>();

        // Contexts
        services.AddSingleton<DeviceConnectionContext>();
        services.AddSingleton<LedstripDisplayContext>();

        // Services
        services.AddScoped<ILedstripService, LedstripService>();
        services.AddScoped<IDeviceService, DeviceService>();

        // Managers
        services.AddTransient<IDeviceManager, DeviceManager>();
        services.AddTransient<IEffectManager, EffectManager>();
        services.AddTransient<ILedstripManager, LedstripManager>();
    }
}