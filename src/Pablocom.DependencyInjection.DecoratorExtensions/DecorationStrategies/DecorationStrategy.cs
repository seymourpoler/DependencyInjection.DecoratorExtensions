namespace Pablocom.DependencyInjection.DecoratorExtensions.DecorationStrategies;

internal abstract class DecorationStrategy
{
    protected Type TypeToDecorate { get; }

    protected DecorationStrategy(Type typeToDecorate)
    {
        TypeToDecorate = typeToDecorate;
    }
    
    public abstract bool CanDecorate(Type type);
    public abstract Func<IServiceProvider, object> CreateImplementationFactory(DecoratedTypeProxy decoratedType);
}