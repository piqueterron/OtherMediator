﻿namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

public static class MediatorExtension
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? config)
    {
        var mediatorConfig = new MediatorConfiguration(services);

        if (config is null)
        {
            mediatorConfig.RegisterServicesFromAssemblies();
        }
        else
        {
            config(mediatorConfig);
        }

        if (mediatorConfig.UseExceptionHandler)
        {
            if (!services.Any((ServiceDescriptor d) => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(ErrorPipelineBehavior<,>)))
            {
                services.Insert(0, ServiceDescriptor.Describe(typeof(IPipelineBehavior<,>), typeof(ErrorPipelineBehavior<,>), mediatorConfig.Lifetime));
            }
        }

        services.AddCoreMediator();

        return services;
    }

    public static IServiceCollection AddMediatorManualRegistration(this IServiceCollection services, Action<IServiceCollection> config)
    {
        config?.Invoke(services);

        services.AddCoreMediator();

        return services;
    }

    private static IServiceCollection AddCoreMediator(this IServiceCollection services)
    {
        services.AddSingleton<IMediator>(sp => new Mediator(new MicrosoftContainer(services), new MiddlewarePipeline()));

        return services;
    }
}