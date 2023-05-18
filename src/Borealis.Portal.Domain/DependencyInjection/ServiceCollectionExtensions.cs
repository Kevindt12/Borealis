using System;
using System.Linq;

using Borealis.Portal.Domain.Effects.Factories;

using Microsoft.Extensions.DependencyInjection;



namespace Borealis.Portal.Domain.DependencyInjection;


public static class ServiceCollectionExtensions
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddTransient<IEffectFactory, EffectFactory>();
    }
}