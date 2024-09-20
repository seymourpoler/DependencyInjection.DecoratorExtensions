using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.UnitTests;

public abstract class DecorationTestBase
{
    protected static ServiceProvider ConfigureServices(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        
        configure.Invoke(services);
        
        return services.BuildServiceProvider();
    }
}