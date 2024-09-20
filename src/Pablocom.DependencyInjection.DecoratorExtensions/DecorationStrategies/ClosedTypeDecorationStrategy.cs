using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.DecorationStrategies;

internal sealed class ClosedTypeDecorationStrategy : DecorationStrategy
{
    private readonly Type _decoratorType;

    public ClosedTypeDecorationStrategy(Type typeToDecorate, Type decoratorType) : base(typeToDecorate)
    {
        _decoratorType = decoratorType;
    }

    public override bool CanDecorate(Type type) => type == TypeToDecorate;

    public override Func<IServiceProvider, object> CreateImplementationFactory(DecoratedTypeProxy decoratedType)
    {
        return serviceProvider =>
        {
            var decoratedTypeInstance = serviceProvider.GetRequiredService(decoratedType);
            return ActivatorUtilities.CreateInstance(serviceProvider, _decoratorType, decoratedTypeInstance);
        };
    }
}