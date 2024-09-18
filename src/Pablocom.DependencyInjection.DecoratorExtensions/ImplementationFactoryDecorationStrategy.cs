using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

internal class ImplementationFactoryDecorationStrategy : IDecorationStrategy
{
    private readonly Func<object, IServiceProvider, object> _decoratorFactory;

    public ImplementationFactoryDecorationStrategy(Func<object, IServiceProvider, object> decoratorFactory)
    {
        _decoratorFactory = decoratorFactory;
    }

    public Func<IServiceProvider, object> CreateDecorator(DecoratedTypeProxy decoratedType)
    {
        return serviceProvider =>
        {
            var decoratedTypeInstance = serviceProvider.GetRequiredService(decoratedType);
            return _decoratorFactory(decoratedTypeInstance, serviceProvider);
        };
    }
}