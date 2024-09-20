using Microsoft.Extensions.DependencyInjection;
using Pablocom.DependencyInjection.DecoratorExtensions.DecorationStrategies;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services) 
        where TDecorator : TDecorated
        where TDecorated : notnull
    {
        var strategy = new ClosedTypeDecorationStrategy(typeof(TDecorated), typeof(TDecorator));
        
        return services.Decorate(strategy);
    }
    
    public static IServiceCollection Decorate(this IServiceCollection services, Type decorated, Type decorator)
    {
        DecorationStrategy strategy = decorated.IsGenericTypeDefinition
            ? new OpenGenericTypeDecorationStrategy(decorated, decorator)
            : new ClosedTypeDecorationStrategy(decorated, decorator);
        
        return services.Decorate(strategy);
    }

    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services, Func<TDecorated, IServiceProvider, TDecorator> decoratorFactory) 
        where TDecorator : TDecorated
        where TDecorated : notnull
    {
        var strategy = new ImplementationFactoryDecorationStrategy(
            typeof(TDecorated), 
            (decoratedInstance, provider) => decoratorFactory((TDecorated)decoratedInstance, provider)
        );
        
        return services.Decorate(strategy);
    }
    
    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services, Func<TDecorated, TDecorator> decoratorFactory) 
        where TDecorator : TDecorated
        where TDecorated : notnull
    {
        var strategy = new ImplementationFactoryDecorationStrategy(
            typeof(TDecorated), 
            (decoratedInstance, _) => decoratorFactory((TDecorated) decoratedInstance)
        );
        
        return services.Decorate(strategy);
    }
    
    public static IServiceCollection Decorate<TDecorated>(this IServiceCollection services, Func<object, IServiceProvider, object> decoratorFactory) 
        where TDecorated : notnull
    {
        var strategy = new ImplementationFactoryDecorationStrategy(typeof(TDecorated), decoratorFactory);
        
        return services.Decorate(strategy);
    }
    
    public static IServiceCollection Decorate<TDecorated>(this IServiceCollection services, Func<TDecorated, IServiceProvider, object> decoratorFactory) 
        where TDecorated : notnull
    {
        var strategy = new ImplementationFactoryDecorationStrategy(
            typeof(TDecorated), 
            (decoratedInstance, provider) => decoratorFactory((TDecorated)decoratedInstance, provider)
        );
        
        return services.Decorate(strategy);
    }

    private static IServiceCollection Decorate(this IServiceCollection services, DecorationStrategy decorationStrategy)
    {
        var hasDecoratedAnyService = false;
        
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var serviceDescriptor = services[i];
            
            if (serviceDescriptor.ServiceType is DecoratedTypeProxy)
                continue;
            
            if (!decorationStrategy.CanDecorate(serviceDescriptor.ServiceType))
                continue;

            var decoratedTypeProxy = new DecoratedTypeProxy(serviceDescriptor.ServiceType);
            
            services.Add(serviceDescriptor.WithServiceType(decoratedTypeProxy));
            services[i] = serviceDescriptor.WithImplementationFactory(
                decorationStrategy.CreateImplementationFactory(decoratedTypeProxy));

            hasDecoratedAnyService = true;
        }

        if (!hasDecoratedAnyService)
            throw new DecorationException(decorationStrategy.TargetDecoratedType);
        
        return services;
    }
}