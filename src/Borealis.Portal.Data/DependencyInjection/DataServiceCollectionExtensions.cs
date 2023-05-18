using System;
using System.Linq;

using Borealis.Portal.Data.Contexts;
using Borealis.Portal.Data.Stores;

using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Data.DependencyInjection;


public static class DataServiceCollectionExtensions
{
    public static void AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>();

        services.AddTransient<IDeviceStore, DeviceStore>();
        services.AddTransient<IEffectStore, EffectStore>();
        services.AddTransient<ILedstripStore, LedstripStore>();
    }
}