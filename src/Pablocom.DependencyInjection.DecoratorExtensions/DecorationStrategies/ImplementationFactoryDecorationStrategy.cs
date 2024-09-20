using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.DecorationStrategies;

internal class ImplementationFactoryDecorationStrategy : DecorationStrategy
    
{
    private readonly Func<object, IServiceProvider, object> _decoratorFactory;

    public ImplementationFactoryDecorationStrategy(Type typeToDecorate, Func<object, IServiceProvider, object> decoratorFactory) : base(typeToDecorate)
    {
        _decoratorFactory = decoratorFactory;
    }

    public override bool CanDecorate(Type type) => type == TypeToDecorate;

    public override Func<IServiceProvider, object> CreateImplementationFactory(DecoratedTypeProxy decoratedType)
    {
        return serviceProvider =>
        {
            var decoratedTypeInstance = serviceProvider.GetRequiredService(decoratedType);
            return _decoratorFactory(decoratedTypeInstance, serviceProvider);
        };
    }
}