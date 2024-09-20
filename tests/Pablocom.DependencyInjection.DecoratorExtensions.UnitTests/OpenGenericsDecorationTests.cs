using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.UnitTests;

public sealed class OpenGenericsDecorationTests : DecorationTestBase
{
    // Test if one is the service type is open generic and the implementation type is closed
    
    [Fact]
    public void DecoratesWithAnOpenGenericType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<IEventHandler<MyEvent>, MyEventHandler>();
            services.Decorate(typeof(IEventHandler<>), typeof(LoggingEventHandlerDecorator<>));
        }); 
        
        var handler = serviceProvider.GetRequiredService<IEventHandler<MyEvent>>();
        
        handler.Handle(new MyEvent());

        handler.Should().BeOfType<LoggingEventHandlerDecorator<MyEvent>>();
        
        var decorator = (LoggingEventHandlerDecorator<MyEvent>) handler;
        decorator.ReceivedCallsCount.Should().Be(1);

        decorator.InnerHandler.Should().BeOfType<MyEventHandler>();
        var decorated = (MyEventHandler) decorator.InnerHandler;
        decorated.ReceivedCallsCount.Should().Be(1);
    }

    [Fact]
    public void DecoratesMultipleServicesWithOpenGenericType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<IEventHandler<MyEvent>, MyEventHandler>();
            services.AddSingleton<IEventHandler<MyOtherEvent>, MyOtherEventHandler>();
            services.Decorate(typeof(IEventHandler<>), typeof(LoggingEventHandlerDecorator<>));
        });
        
        var handler1 = serviceProvider.GetRequiredService<IEventHandler<MyEvent>>();
        var handler2 = serviceProvider.GetRequiredService<IEventHandler<MyOtherEvent>>();
        
        handler1.Handle(new MyEvent());
        handler2.Handle(new MyOtherEvent());
        
        handler1.Should().BeOfType<LoggingEventHandlerDecorator<MyEvent>>();
        handler2.Should().BeOfType<LoggingEventHandlerDecorator<MyOtherEvent>>();
        
        var decorator1 = (LoggingEventHandlerDecorator<MyEvent>) handler1;
        var decorator2 = (LoggingEventHandlerDecorator<MyOtherEvent>) handler2;
        
        decorator1.ReceivedCallsCount.Should().Be(1);
        decorator2.ReceivedCallsCount.Should().Be(1);
        decorator1.InnerHandler.As<MyEventHandler>().ReceivedCallsCount.Should().Be(1);
        decorator2.InnerHandler.As<MyOtherEventHandler>().ReceivedCallsCount.Should().Be(1);
    }
    
    [Fact]
    public void DecoratesMultipleLevelsOfClosedTypesWithOpenGenericType()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<IEventHandler<MyEvent>, MyEventHandler>();
            services.Decorate(typeof(IEventHandler<MyEvent>), typeof(LoggingEventHandlerDecorator<MyEvent>));
            services.Decorate(typeof(IEventHandler<>), typeof(LoggingEventHandlerDecorator<>));
        });
        
        var handler = serviceProvider.GetRequiredService<IEventHandler<MyEvent>>();
        handler.Handle(new MyEvent());
        
        handler.Should().BeOfType<LoggingEventHandlerDecorator<MyEvent>>();
        
        var decorator = (LoggingEventHandlerDecorator<MyEvent>) handler;
        
        decorator.InnerHandler.Should().BeOfType<LoggingEventHandlerDecorator<MyEvent>>();
        decorator.InnerHandler.As<LoggingEventHandlerDecorator<MyEvent>>().InnerHandler.Should().BeOfType<MyEventHandler>();
    }

    [Fact]
    public void DoesNotDecorateNonAssignableClosedGenericTypes()
    {
        var serviceProvider = ConfigureServices(services =>
        {
            services.AddSingleton<IEventHandler<MyEvent>, MyEventHandler>();
            services.AddSingleton<IEventHandler<MyOtherEvent>, MyOtherEventHandler>();
            services.Decorate(typeof(IEventHandler<MyOtherEvent>), typeof(LoggingEventHandlerDecorator<MyOtherEvent>));
        });
        
        var handler = serviceProvider.GetRequiredService<IEventHandler<MyEvent>>();
        
        handler.Should().BeOfType<MyEventHandler>();
    }
    
    public interface IEvent;

    public sealed class MyEvent : IEvent;
    
    public sealed class MyOtherEvent : IEvent;

    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        void Handle(TEvent @event);
    }

    public sealed class MyEventHandler : IEventHandler<MyEvent>
    {
        public int ReceivedCallsCount { get; private set; }
        
        public void Handle(MyEvent @event) => ReceivedCallsCount++;
    }
    
    public sealed class MyOtherEventHandler : IEventHandler<MyOtherEvent>
    {
        public int ReceivedCallsCount { get; private set; }
        
        public void Handle(MyOtherEvent @event) => ReceivedCallsCount++;
    }
    
    public sealed class LoggingEventHandlerDecorator<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        public IEventHandler<TEvent> InnerHandler { get; }
        public int ReceivedCallsCount { get; private set; }

        public LoggingEventHandlerDecorator(IEventHandler<TEvent> innerHandler)
        {
            InnerHandler = innerHandler;
        }
        
        public void Handle(TEvent @event)
        {
            ReceivedCallsCount++;
            InnerHandler.Handle(@event);
        }
    }
}