using System;

using Borealis.Portal.Data.Extensions;
using Borealis.Portal.Domain.Extensions;
using Borealis.Portal.Infrastructure.Extensions;

using System.Linq;

using Borealis.Portal.Core.Animations;
using Borealis.Portal.Core.Devices;
using Borealis.Portal.Core.Effects;
using Borealis.Portal.Core.Interaction;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Devices;
using Borealis.Portal.Domain.Effects;

using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Core.Extensions;


public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Dependencies
        services.AddDomainServices();
        services.AddInfrastructureServices();
        services.AddDataServices();

        // Contexts
        services.AddSingleton<LedstripContext>();
        services.AddSingleton<DeviceContext>();

        // Services
        services.AddScoped<ILedstripService, LedstripService>();
        services.AddScoped<IDeviceService, DeviceService>();

        // Managers
        services.AddTransient<IAnimationManager, AnimationManager>();
        services.AddTransient<IDeviceManager, DeviceManager>();
        services.AddTransient<IEffectManager, EffectManager>();

        // Factories
        services.AddTransient<AnimationPlayerFactory>();
        services.AddTransient<SolidColorInteractorFactory>();
    }
}