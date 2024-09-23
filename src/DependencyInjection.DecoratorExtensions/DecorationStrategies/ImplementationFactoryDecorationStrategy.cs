using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.DecoratorExtensions.DecorationStrategies;

internal class ImplementationFactoryDecorationStrategy : DecorationStrategy
    
{
    private readonly Func<object, IServiceProvider, object> _decoratorFactory;

    public ImplementationFactoryDecorationStrategy(Type decoratedType, Func<object, IServiceProvider, object> decoratorFactory) : base(decoratedType)
    {
        _decoratorFactory = decoratorFactory;
    }

    public override bool CanDecorate(Type type) => type == TargetDecoratedType;

    public override Func<IServiceProvider, object> CreateImplementationFactory(DecoratedTypeProxy decoratedType)
    {
        return serviceProvider =>
        {
            var decoratedTypeInstance = serviceProvider.GetRequiredService(decoratedType);
            return _decoratorFactory(decoratedTypeInstance, serviceProvider);
        };
    }
}