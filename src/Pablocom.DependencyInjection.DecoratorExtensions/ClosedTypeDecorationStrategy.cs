using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions;

internal sealed class OpenGenericTypeDecorationStrategy : DecorationStrategy
{
    private readonly Type _decoratorType;

    public OpenGenericTypeDecorationStrategy(Type typeToDecorate, Type decoratorType) : base(typeToDecorate)
    {
        _decoratorType = decoratorType;
    }

    public override bool CanDecorate(Type type)
    {
        return type is { IsGenericType: true, IsGenericTypeDefinition: false }
               && type.GetGenericTypeDefinition() == TypeToDecorate.GetGenericTypeDefinition()
               && (_decoratorType is null || GenericArgumentsAreCompatible(type, _decoratorType));
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