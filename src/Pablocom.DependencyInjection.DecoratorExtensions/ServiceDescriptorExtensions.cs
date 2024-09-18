using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

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
    
    public static ServiceDescriptor WithImplementationFactory(this ServiceDescriptor descriptor, 
        Func<IServiceProvider, object> implementationFactory)
    {
        return new ServiceDescriptor(descriptor.ServiceType, factory: implementationFactory, descriptor.Lifetime);
    }
}