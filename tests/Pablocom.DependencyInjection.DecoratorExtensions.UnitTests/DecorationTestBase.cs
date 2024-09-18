using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.UnitTests;

public abstract class DecorationTestBase
{
    protected static ServiceProvider ConfigureServices(Action<IServiceCollection> configurationDelegate)
    {
        var services = new ServiceCollection();
        
        configurationDelegate.Invoke(services);
        
        return services.BuildServiceProvider();
    }
}