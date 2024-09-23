using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.DecoratorExtensions.DecorationStrategies;

internal sealed class ClosedTypeDecorationStrategy : DecorationStrategy
{
    private readonly Type _decoratorType;

    public ClosedTypeDecorationStrategy(Type decoratedType, Type decoratorType) : base(decoratedType)
    {
        if (decoratedType.IsGenericTypeDefinition)
            throw new ArgumentException(
                "Cannot create closed type decoration strategy if any of the types is open generic", nameof(decoratedType)); 
        
        if (decoratorType.IsGenericTypeDefinition)
            throw new ArgumentException(
                "Cannot create closed type decoration strategy if any of the types is open generic", nameof(decoratorType));
        
        _decoratorType = decoratorType;
    }

    public override bool CanDecorate(Type type) => type == TargetDecoratedType;

    public override Func<IServiceProvider, object> CreateImplementationFactory(DecoratedTypeProxy decoratedType)
    {
        return serviceProvider =>
        {
            var decoratedTypeInstance = serviceProvider.GetRequiredService(decoratedType);
            return ActivatorUtilities.CreateInstance(serviceProvider, _decoratorType, decoratedTypeInstance);
        };
    }
}