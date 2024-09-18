namespace Pablocom.DependencyInjection.DecoratorExtensions;

internal interface IDecorationStrategy
{
    public Func<IServiceProvider, object> CreateDecorator(DecoratedTypeProxy decoratedType);
}