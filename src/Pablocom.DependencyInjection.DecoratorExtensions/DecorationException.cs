namespace Pablocom.DependencyInjection.DecoratorExtensions;

public class DecorationException : InvalidOperationException
{
    public Type ServiceType { get; }
    
    public DecorationException(Type serviceType) 
        : base($"Could not find any registered service to decorate for type '{serviceType.FullName}'.")
    {
        ServiceType = serviceType;
    }
}