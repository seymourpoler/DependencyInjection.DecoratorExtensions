using Microsoft.Extensions.DependencyInjection;
using Pablocom.DependencyInjection.DecoratorExtensions.DecorationStrategies;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services) 
        where TDecorator : TDecorated
    {
        var strategy = new ClosedTypeDecorationStrategy(typeof(TDecorated), typeof(TDecorator));
        
        if (!services.TryDecorate(strategy)) 
            throw new DecorationException(typeof(TDecorated));
        
        return services;
    }
    
    public static IServiceCollection Decorate(this IServiceCollection services, Type decorated, Type decorator)
    {
        DecorationStrategy strategy = decorated.IsGenericTypeDefinition
            ? new OpenGenericTypeDecorationStrategy(decorated, decorator)
            : new ClosedTypeDecorationStrategy(decorated, decorator);

        if (!services.TryDecorate(strategy)) 
            throw new DecorationException(decorated);
        
        return services;
    }

    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services, 
        Func<TDecorated, IServiceProvider, TDecorator> decoratorFactory) 
        where TDecorator : TDecorated
        where TDecorated : notnull
    {
        var strategy = new ImplementationFactoryDecorationStrategy(
            typeof(TDecorated), 
            (decoratedInstance, provider) => decoratorFactory((TDecorated) decoratedInstance, provider)
        );
        
        if (!services.TryDecorate(strategy)) 
            throw new DecorationException(typeof(TDecorated));
        
        return services;
    }
    
    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services, 
        Func<TDecorated, TDecorator> decoratorFactory) 
        where TDecorator : TDecorated
        where TDecorated : notnull
    {
        var strategy = new ImplementationFactoryDecorationStrategy(
            typeof(TDecorated), 
            (decoratedInstance, _) => decoratorFactory((TDecorated) decoratedInstance)
        );
        
        if (!services.TryDecorate(strategy)) 
            throw new DecorationException(typeof(TDecorated));
        
        return services;
    }
    
    public static IServiceCollection Decorate<TDecorated>(this IServiceCollection services, 
        Func<object, IServiceProvider, object> decoratorFactory) 
        where TDecorated : class
    {
        var strategy = new ImplementationFactoryDecorationStrategy(typeof(TDecorated), decoratorFactory);
        
        if (!services.TryDecorate(strategy)) 
            throw new DecorationException(typeof(TDecorated));
        
        return services;
    }
    
    public static IServiceCollection Decorate<TDecorated>(this IServiceCollection services, 
        Func<TDecorated, IServiceProvider, object> decoratorFactory) 
        where TDecorated : class
    {
        var strategy = new ImplementationFactoryDecorationStrategy(
            typeof(TDecorated), 
            (decoratedInstance, provider) => decoratorFactory((TDecorated) decoratedInstance, provider)
        );

        if (!services.TryDecorate(strategy)) 
            throw new DecorationException(typeof(TDecorated));
        
        return services;
    }

    private static bool TryDecorate(this IServiceCollection services, DecorationStrategy decorationStrategy)
    {
        var isDecorated = false;
        
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
            
            isDecorated = true;
        }
        
        return isDecorated;
    }
}