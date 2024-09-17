using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services) where TDecorator : TService
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var serviceDescriptor = services[i];
            if (serviceDescriptor.ServiceType != typeof(TService))
                continue;

            var decoratedServiceDescriptor = serviceDescriptor.WithServiceType(new DecoratedTypeProxy(serviceDescriptor.ServiceType));
            
            services.Add(decoratedServiceDescriptor);

            services[i] = new ServiceDescriptor(
                typeof(TService), 
                factory: sp => ActivatorUtilities.CreateInstance(sp, typeof(TDecorator), sp.GetRequiredService(decoratedServiceDescriptor.ServiceType)), 
                serviceDescriptor.Lifetime
            );
        }
        
        return services;
    }
}

internal static class ServiceDescriptorExtensions
{
    public static ServiceDescriptor WithServiceType(this ServiceDescriptor descriptor, Type decoratedType)
    {
        if (descriptor.ImplementationType is not null)
            return new ServiceDescriptor(decoratedType, descriptor.ImplementationType, descriptor.Lifetime);
        
        if (descriptor.ImplementationFactory is not null)
            return new ServiceDescriptor(decoratedType, descriptor.ImplementationFactory, descriptor.Lifetime);
            
        throw new ArgumentException("The service descriptor does not have implementation type neither implementation factory.");
    }
}