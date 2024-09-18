using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

internal class TypeDecoratorDecorationStrategy : IDecorationStrategy
{
    private readonly Type _decoratorType;

    public TypeDecoratorDecorationStrategy(Type decoratorType)
    {
        _decoratorType = decoratorType;
    }

    public Func<IServiceProvider, object> CreateDecorator(DecoratedTypeProxy decoratedType)
    {
        return serviceProvider =>
        {
            var decoratedTypeInstance = serviceProvider.GetRequiredService(decoratedType);
            return ActivatorUtilities.CreateInstance(serviceProvider, _decoratorType, decoratedTypeInstance);
        };
    }
}