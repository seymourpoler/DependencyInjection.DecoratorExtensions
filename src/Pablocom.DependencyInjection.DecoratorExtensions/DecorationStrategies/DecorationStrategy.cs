namespace Pablocom.DependencyInjection.DecoratorExtensions.DecorationStrategies;

internal abstract class DecorationStrategy
{
    protected Type TargetDecoratedType { get; }

    protected DecorationStrategy(Type decoratedType)
    {
        TargetDecoratedType = decoratedType;
    }
    
    public abstract bool CanDecorate(Type type);
    public abstract Func<IServiceProvider, object> CreateImplementationFactory(DecoratedTypeProxy decoratedType);
}