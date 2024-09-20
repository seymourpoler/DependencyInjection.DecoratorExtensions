using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.DecorationStrategies;

internal sealed class OpenGenericTypeDecorationStrategy : DecorationStrategy
{
    private readonly Type _decoratorType;

    public OpenGenericTypeDecorationStrategy(Type decoratedType, Type decoratorType) : base(decoratedType)
    {
        if (!decoratedType.IsGenericTypeDefinition)
            throw new ArgumentException(
                "Cannot create open generic decoration strategy if any of the types is not open generic", nameof(decoratedType));
        
        if (!decoratorType.IsGenericTypeDefinition)
            throw new ArgumentException(
                "Cannot create open generic decoration strategy if any of the types is not open generic", nameof(decoratorType));
        
        _decoratorType = decoratorType;
    }

    public override bool CanDecorate(Type type)
    {
        return type is { IsGenericType: true, IsGenericTypeDefinition: false }
               && type.GetGenericTypeDefinition() == TargetDecoratedType.GetGenericTypeDefinition()
               && GenericArgumentsAreCompatible(type, _decoratorType);
    }

    public override Func<IServiceProvider, object> CreateImplementationFactory(DecoratedTypeProxy decoratedType)
    {
        var genericArguments = decoratedType.GetGenericArguments();
        
        return serviceProvider =>
        {
            var decoratedTypeInstance = serviceProvider.GetRequiredService(decoratedType);
            return ActivatorUtilities.CreateInstance(serviceProvider, _decoratorType.MakeGenericType(genericArguments), decoratedTypeInstance);
        };
    }
    
    private static bool GenericArgumentsAreCompatible(Type type, Type otherType)
    {
        var genericArguments = type.GetGenericArguments();
        try
        {
            _ = otherType.MakeGenericType(genericArguments);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}