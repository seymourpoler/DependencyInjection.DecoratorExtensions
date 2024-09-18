using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.UnitTests;

public sealed class ClosedTypesDecorationTests : DecorationTestBase
{
    [Fact]
    public void CanDecorateType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddTransient<IDecoratedService, Decorated>();
            services.Decorate<IDecoratedService, Decorator>();
        });

        var resolvedService = serviceProvider.GetRequiredService<IDecoratedService>();
        
        resolvedService.Execute();
        
        resolvedService.Should().BeOfType<Decorator>();
        
        var decorator = (Decorator) resolvedService;
        decorator.InnerDecoratedService.Should().BeOfType<Decorated>();
        
        var decorated = (Decorated) decorator.InnerDecoratedService;
        
        decorator.ReceivedCallsCount.Should().Be(1);
        decorated.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void CanDecorateTypeFromImplementationFactory()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddTransient<IDecoratedService> (_ => new Decorated());
            services.Decorate<IDecoratedService, Decorator>();
        });

        var resolvedService = serviceProvider.GetRequiredService<IDecoratedService>();
        
        resolvedService.Execute();
        
        resolvedService.Should().BeOfType<Decorator>();
        
        var decorator = (Decorator) resolvedService;
        decorator.InnerDecoratedService.Should().BeOfType<Decorated>();
        
        var decorated = (Decorated) decorator.InnerDecoratedService;
        
        decorator.ReceivedCallsCount.Should().Be(1);
        decorated.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void CanDecorateMultipleLevels()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddTransient<IDecoratedService, Decorated>();
            services.Decorate<IDecoratedService, Decorator>();
            services.Decorate<IDecoratedService, Decorator>();
        });

        var resolvedService = serviceProvider.GetRequiredService<IDecoratedService>();
        
        resolvedService.Execute();
        
        resolvedService.Should().BeOfType<Decorator>();
        
        var outerDecorator = (Decorator)resolvedService;
        
        outerDecorator.InnerDecoratedService.Should().BeOfType<Decorator>();
        
        var innerDecorator = (Decorator)outerDecorator.InnerDecoratedService;
        
        outerDecorator.ReceivedCallsCount.Should().Be(1);
        innerDecorator.ReceivedCallsCount.Should().Be(1);
        
        innerDecorator.InnerDecoratedService.Should().BeOfType<Decorated>();
        var innerService = (Decorated)innerDecorator.InnerDecoratedService;
        innerService.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void DecoratesAllServicesOfTheSameType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<IDecoratedService, Decorated>();
            services.AddSingleton<IDecoratedService, Decorated>();
            
            services.Decorate<IDecoratedService, Decorator>();
        });

        var resolvedServices = serviceProvider.GetServices<IDecoratedService>().ToArray();
        
        resolvedServices.Should().HaveCount(2);
        resolvedServices.Should().AllBeOfType<Decorator>();
        resolvedServices.OfType<Decorator>().Should().AllSatisfy(x => x.InnerDecoratedService.Should().BeOfType<Decorated>());
    }

    [Fact]
    public void DecoratesUsingImplementationFactory()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddTransient<IDecoratedService, Decorated>();
            services.Decorate<IDecoratedService, Decorator>((decoratedService, _) => new Decorator(decoratedService));
        });

        var resolvedService = serviceProvider.GetRequiredService<IDecoratedService>();
        
        resolvedService.Execute();
        
        resolvedService.Should().BeOfType<Decorator>();
        
        var decorator = (Decorator) resolvedService;
        decorator.InnerDecoratedService.Should().BeOfType<Decorated>();
        
        var decorated = (Decorated) decorator.InnerDecoratedService;
        decorator.ReceivedCallsCount.Should().Be(1);
        decorated.ReceivedCallsCount.Should().Be(1);
    }

    [Fact]
    public void WorksWithServiceScopes()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddScoped<IDecoratedService, Decorated>();
            services.Decorate<IDecoratedService, Decorator>();
        });

        using var scope = serviceProvider.CreateScope();
        var decoratedService = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
        decoratedService.Execute();
        
        var sameDecoratedServiceInstance = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
        
        sameDecoratedServiceInstance.Should().BeOfType<Decorator>();
        
        var decorator = (Decorator) sameDecoratedServiceInstance;
        decorator.InnerDecoratedService.Should().BeOfType<Decorated>();
        decorator.ReceivedCallsCount.Should().Be(1);
        
        var decorated = (Decorated) decorator.InnerDecoratedService;
        decorated.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void AllowsConfiguringDecoratorsPassingTheTypeOrImplementationFactory_New()
    {
        var allDecorationMethods = new Action<IServiceCollection>[]
        {
            services => services.Decorate<IDecoratedService, Decorator>(),
            services => services.Decorate(typeof(IDecoratedService), typeof(Decorator)),
            services => services.Decorate<IDecoratedService, Decorator>((decorated, _) => new Decorator(decorated)),
            services => services.Decorate<IDecoratedService>((decorated, _) => new Decorator(decorated)),
            services => services.Decorate((IDecoratedService decorated, IServiceProvider _) => new Decorator(decorated)),
            services => services.Decorate((IDecoratedService decorated) => new Decorator(decorated))
        };

        foreach (var decorationMethod in allDecorationMethods)
        {
            var provider = ConfigureServices(services =>
            {
                services.AddSingleton<IDecoratedService, Decorated>();
                decorationMethod(services);
            });
            
            var instance = provider.GetRequiredService<IDecoratedService>();
            instance.Should().BeOfType<Decorator>();
            
            var decorator = (Decorator) instance;
            decorator.InnerDecoratedService.Should().BeOfType<Decorated>();
        }
    }

    public interface IDecoratedService
    {
        void Execute();
    }

    public class Decorated : IDecoratedService
    {
        public int ReceivedCallsCount { get; private set; }
        
        public void Execute()
        {
            ReceivedCallsCount++;
        }
    }

    public class Decorator : IDecoratedService
    {
        public int ReceivedCallsCount { get; private set; }
        public IDecoratedService InnerDecoratedService { get; }

        public Decorator(IDecoratedService innerDecoratedService)
        {
            InnerDecoratedService = innerDecoratedService;
        }
        
        public void Execute()
        {
            ReceivedCallsCount++;
            InnerDecoratedService.Execute();
        }
    }
}