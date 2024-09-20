using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.UnitTests;

public sealed class WhenDecoratingServices
{
    [Fact]
    public void CanDecorateServices()
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
        
        resolvedService.As<Decorator>().InnerDecoratedService.Should().BeOfType<Decorated>();
        
        var decorated = (Decorated) decorator.InnerDecoratedService;
        
        decorator.ReceivedCallsCount.Should().Be(1);
        decorated.ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void CanDecorateServiceRegisteredWithImplementationFactory()
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
        
        innerDecorator.InnerDecoratedService.Should().BeOfType<Decorated>();
        
        var decorated = (Decorated)innerDecorator.InnerDecoratedService;
        
        outerDecorator.ReceivedCallsCount.Should().Be(1);
        innerDecorator.ReceivedCallsCount.Should().Be(1);
        decorated.ReceivedCallsCount.Should().Be(1);
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

        foreach (var service in resolvedServices)
        {
            service.Execute();
        }
        
        resolvedServices.Should().HaveCount(2);
        resolvedServices.Should().AllBeOfType<Decorator>();
        
        resolvedServices.OfType<Decorator>().Should().AllSatisfy(x =>
        {
            x.InnerDecoratedService.Should().BeOfType<Decorated>();
                
            x.ReceivedCallsCount.Should().Be(1);
            x.InnerDecoratedService.As<Decorated>().ReceivedCallsCount.Should().Be(1);
        });
    }
    
    [Fact]
    public void DecoratesScopedServices()
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
    public void AllowsConfiguringDecoratorsUsingDifferentOverloads()
    {
        var allDecorationOverloads = new Action<IServiceCollection>[]
        {
            services => services.Decorate<IDecoratedService, Decorator>(),
            services => services.Decorate(typeof(IDecoratedService), typeof(Decorator)),
            services => services.Decorate<IDecoratedService, Decorator>((decorated, _) => new Decorator(decorated)),
            services => services.Decorate<IDecoratedService>((decorated, _) => new Decorator(decorated)),
            services => services.Decorate((IDecoratedService decorated, IServiceProvider _) => new Decorator(decorated)),
            services => services.Decorate<IDecoratedService>((decorated, _) => new Decorator((IDecoratedService)decorated)),
            services => services.Decorate((IDecoratedService decorated) => new Decorator(decorated))
        };

        foreach (var decorationOverload in allDecorationOverloads)
        {
            var provider = ConfigureServices(services =>
            {
                services.AddSingleton<IDecoratedService, Decorated>();
                decorationOverload(services);
            });
            
            var instance = provider.GetRequiredService<IDecoratedService>();
            instance.Should().BeOfType<Decorator>();
            instance.As<Decorator>().InnerDecoratedService.Should().BeOfType<Decorated>();
        }
    }

    [Fact]
    public void ThrowsExceptionIfCannotFindServiceToDecorate()
    {
        var services = new ServiceCollection();

        var act = () => services.Decorate<IDecoratedService, Decorator>();
        
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void DecoratesOpenGenericTypes()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<ICommandHandler<MyCommand>, MyCommandHandler>();
            services.Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
        }); 
        
        var handler = serviceProvider.GetRequiredService<ICommandHandler<MyCommand>>();
        
        handler.Handle(new MyCommand());

        handler.Should().BeOfType<CommandHandlerDecorator<MyCommand>>();
        
        var decorator = (CommandHandlerDecorator<MyCommand>) handler;
        decorator.ReceivedCallsCount.Should().Be(1);

        decorator.InnerHandler.Should().BeOfType<MyCommandHandler>();
        var decorated = (MyCommandHandler) decorator.InnerHandler;
        decorated.ReceivedCallsCount.Should().Be(1);
    }

    [Fact]
    public void DecoratesMultipleServicesWithOpenGenericType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<ICommandHandler<MyCommand>, MyCommandHandler>();
            services.AddSingleton<ICommandHandler<MyOtherCommand>, MyOtherCommandHandler>();
            services.Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
        });
        
        var handler1 = serviceProvider.GetRequiredService<ICommandHandler<MyCommand>>();
        var handler2 = serviceProvider.GetRequiredService<ICommandHandler<MyOtherCommand>>();
        
        handler1.Handle(new MyCommand());
        handler2.Handle(new MyOtherCommand());
        
        handler1.Should().BeOfType<CommandHandlerDecorator<MyCommand>>();
        handler2.Should().BeOfType<CommandHandlerDecorator<MyOtherCommand>>();
        
        var decorator1 = (CommandHandlerDecorator<MyCommand>) handler1;
        var decorator2 = (CommandHandlerDecorator<MyOtherCommand>) handler2;
        
        decorator1.ReceivedCallsCount.Should().Be(1);
        decorator2.ReceivedCallsCount.Should().Be(1);
        decorator1.InnerHandler.As<MyCommandHandler>().ReceivedCallsCount.Should().Be(1);
        decorator2.InnerHandler.As<MyOtherCommandHandler>().ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void DecoratesMultipleLevelsOfClosedTypesWithOpenGenericType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<ICommandHandler<MyCommand>, MyCommandHandler>();
            services.Decorate(typeof(ICommandHandler<MyCommand>), typeof(CommandHandlerDecorator<MyCommand>));
            services.Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
        });
        
        var handler = serviceProvider.GetRequiredService<ICommandHandler<MyCommand>>();
        
        handler.Should().BeOfType<CommandHandlerDecorator<MyCommand>>();
        
        var decorator = (CommandHandlerDecorator<MyCommand>) handler;
        
        decorator.InnerHandler.Should().BeOfType<CommandHandlerDecorator<MyCommand>>();
        decorator.InnerHandler.As<CommandHandlerDecorator<MyCommand>>().InnerHandler.Should().BeOfType<MyCommandHandler>();
    }

    [Fact]
    public void DecoratesTypesOnlyWithCompatibleTypeRestrictions()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<ICommandHandler<MyCommand>, MyCommandHandler>();
            services.AddSingleton<ICommandHandler<MyCommandSubType>, MyCommandSubTypeHandler>();
            services.Decorate(typeof(ICommandHandler<>), typeof(CommandSubTypeHandlerDecorator<>));
        });
        
        var nonDecoratedHandler = serviceProvider.GetRequiredService<ICommandHandler<MyCommand>>();
        var decoratedHandler = serviceProvider.GetRequiredService<ICommandHandler<MyCommandSubType>>();
        
        nonDecoratedHandler.Should().BeOfType<MyCommandHandler>();
        
        decoratedHandler.Should().BeOfType<CommandSubTypeHandlerDecorator<MyCommandSubType>>();
        decoratedHandler.As<CommandSubTypeHandlerDecorator<MyCommandSubType>>().InnerHandler.Should().BeOfType<MyCommandSubTypeHandler>();
    }

    [Fact]
    public void ThrowsArgumentExceptionIfOnlyTheDecoratorTypeIsOpenGeneric()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandHandler<MyCommand>, MyCommandHandler>();
        
        var act = () => services.Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<MyCommand>));

        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void ThrowsArgumentExceptionIfOnlyTheDecoratedTypeIsClosedType()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandHandler<MyCommand>, MyCommandHandler>();
        
        var act = () => services.Decorate(typeof(ICommandHandler<MyCommand>), typeof(CommandHandlerDecorator<>));

        act.Should().Throw<ArgumentException>();
    }
    
    private static ServiceProvider ConfigureServices(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        
        configure.Invoke(services);
        
        return services.BuildServiceProvider();
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
    
    public interface ICommand;

    public sealed class MyCommand : ICommand;
    
    public sealed class MyOtherCommand : ICommand;
    
    public interface ICommandSubType : ICommand;
    
    public sealed class MyCommandSubType : ICommandSubType;
    
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }

    public sealed class MyCommandHandler : ICommandHandler<MyCommand>
    {
        public int ReceivedCallsCount { get; private set; }
        
        public void Handle(MyCommand command) => ReceivedCallsCount++;
    }
    
    public sealed class MyOtherCommandHandler : ICommandHandler<MyOtherCommand>
    {
        public int ReceivedCallsCount { get; private set; }
        
        public void Handle(MyOtherCommand command) => ReceivedCallsCount++;
    }
    
    public sealed class MyCommandSubTypeHandler : ICommandHandler<MyCommandSubType>
    {
        public void Handle(MyCommandSubType command) { }
    }
    
    public sealed class CommandHandlerDecorator<TEvent> : ICommandHandler<TEvent> where TEvent : ICommand
    {
        public ICommandHandler<TEvent> InnerHandler { get; }
        public int ReceivedCallsCount { get; private set; }

        public CommandHandlerDecorator(ICommandHandler<TEvent> innerHandler)
        {
            InnerHandler = innerHandler;
        }
        
        public void Handle(TEvent command)
        {
            ReceivedCallsCount++;
            InnerHandler.Handle(command);
        }
    }
    public sealed class CommandSubTypeHandlerDecorator<TCommand> : ICommandHandler<TCommand> where TCommand : ICommandSubType
    {
        public ICommandHandler<TCommand> InnerHandler { get; }

        public CommandSubTypeHandlerDecorator(ICommandHandler<TCommand> innerHandler) => InnerHandler = innerHandler;

        public void Handle(TCommand command) => InnerHandler.Handle(command);
    }
}