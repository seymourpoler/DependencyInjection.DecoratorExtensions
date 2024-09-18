using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services) 
        where TDecorator : TDecorated
    {
        return services.Decorate<TDecorated>(new TypeDecoratorDecorationStrategy(typeof(TDecorator)));
    }
    
    public static IServiceCollection Decorate(this IServiceCollection services, Type decorated, Type decorator) 
    {
        return services.Decorate(decorated, new TypeDecoratorDecorationStrategy(decorator));
    }

    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services, Func<TDecorated, IServiceProvider, TDecorator> decoratorFactory) 
        where TDecorator : TDecorated
        where TDecorated : notnull
    {
        return services.Decorate<TDecorated>(
            new ImplementationFactoryDecorationStrategy((decoratedInstance, provider) => decoratorFactory((TDecorated)decoratedInstance, provider)));
    }
    
    public static IServiceCollection Decorate<TDecorated, TDecorator>(this IServiceCollection services, Func<TDecorated, TDecorator> decoratorFactory) 
        where TDecorator : TDecorated
        where TDecorated : notnull
    {
        return services.Decorate<TDecorated>(
            new ImplementationFactoryDecorationStrategy((decoratedInstance, _) => decoratorFactory((TDecorated)decoratedInstance)));
    }
    
    public static IServiceCollection Decorate<TDecorated>(this IServiceCollection services, Func<object, IServiceProvider, object> decoratorFactory) 
        where TDecorated : class 
    {
        return services.Decorate<TDecorated>(new ImplementationFactoryDecorationStrategy(decoratorFactory));
    }
    
    public static IServiceCollection Decorate<TDecorated>(this IServiceCollection services, Func<TDecorated, IServiceProvider, object> decoratorFactory) 
        where TDecorated : class 
    {
        return services.Decorate<TDecorated>(
            new ImplementationFactoryDecorationStrategy((decoratedInstance, provider) => decoratorFactory((TDecorated)decoratedInstance, provider)));
    }

    private static IServiceCollection Decorate<TDecorated>(this IServiceCollection services, IDecorationStrategy decorationStrategy)
    {
        return services.Decorate(typeof(TDecorated), decorationStrategy);
    }

    private static IServiceCollection Decorate(this IServiceCollection services, Type decoratedType, IDecorationStrategy decorationStrategy)
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var serviceDescriptor = services[i];
            
            if (serviceDescriptor.ServiceType == typeof(DecoratedTypeProxy))
                continue;
            
            if (serviceDescriptor.ServiceType != decoratedType)
                continue;

            var decoratedTypeProxy = new DecoratedTypeProxy(serviceDescriptor.ServiceType);
            
            services.Add(serviceDescriptor.WithServiceType(decoratedTypeProxy));
            services[i] = serviceDescriptor.WithImplementationFactory(decorationStrategy.CreateDecorator(decoratedTypeProxy));
        }
        
        return services;
    }
}