using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.UnitTests;

public sealed class DecoratorExtensionsTests
{
    [Fact]
    public void CanDecorateType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddTransient<IService, MyService>();
            services.Decorate<IService, MyServiceDecorator>();
        });

        var resolvedService = serviceProvider.GetRequiredService<IService>();
        
        resolvedService.Execute();
        
        resolvedService.Should().BeOfType<MyServiceDecorator>();
        
        var decorator = (MyServiceDecorator) resolvedService;
        decorator.InnerService.Should().BeOfType<MyService>();
        
        var decorated = (MyService) decorator.InnerService;
        
        decorator.ReceivedCallsCount.Should().Be(1);
        decorated.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void CanDecorateTypeFromImplementationFactory()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddTransient<IService> (_ => new MyService());
            services.Decorate<IService, MyServiceDecorator>();
        });

        var resolvedService = serviceProvider.GetRequiredService<IService>();
        
        resolvedService.Execute();
        
        resolvedService.Should().BeOfType<MyServiceDecorator>();
        
        var decorator = (MyServiceDecorator) resolvedService;
        decorator.InnerService.Should().BeOfType<MyService>();
        
        var decorated = (MyService) decorator.InnerService;
        
        decorator.ReceivedCallsCount.Should().Be(1);
        decorated.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void CanDecorateMultipleLevels()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddTransient<IService, MyService>();
            services.Decorate<IService, MyServiceDecorator>();
            services.Decorate<IService, MyServiceDecorator>();
        });

        var resolvedService = serviceProvider.GetRequiredService<IService>();
        
        resolvedService.Execute();
        
        resolvedService.Should().BeOfType<MyServiceDecorator>();
        
        var outerDecorator = (MyServiceDecorator)resolvedService;
        outerDecorator.InnerService.Should().BeOfType<MyServiceDecorator>();
        
        var innerDecorator = (MyServiceDecorator)outerDecorator.InnerService;
        
        outerDecorator.ReceivedCallsCount.Should().Be(1);
        innerDecorator.ReceivedCallsCount.Should().Be(1);
        innerDecorator.InnerService.Should().BeOfType<MyService>();

        var innerService = (MyService)innerDecorator.InnerService;
        innerService.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void DecoratesAllServicesOfTheSameType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<IService, MyService>();
            services.AddSingleton<IService, MyService>();
            services.Decorate<IService, MyServiceDecorator>();
        });

        var resolvedServices = serviceProvider.GetServices<IService>();
        
        Assert.Fail("WIP");
    }

    private static IServiceProvider ConfigureServices(Action<IServiceCollection>? configurationDelegate = null)
    {
        var services = new ServiceCollection();
        
        configurationDelegate?.Invoke(services);
        
        return services.BuildServiceProvider();
    }

    public interface IService
    {
        void Execute();
    }

    public class MyService : IService
    {
        public int ReceivedCallsCount { get; private set; }
        
        public void Execute()
        {
            ReceivedCallsCount++;
        }
    }
    
    public class MyServiceDecorator : IService
    {
        public int ReceivedCallsCount { get; private set; }
        public IService InnerService { get; }

        public MyServiceDecorator(IService innerService)
        {
            InnerService = innerService;
        }
        
        public void Execute()
        {
            ReceivedCallsCount++;
            InnerService.Execute();
        }
    }
}