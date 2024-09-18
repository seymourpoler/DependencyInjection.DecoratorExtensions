using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Pablocom.DependencyInjection.DecoratorExtensions.UnitTests;

public sealed class OpenGenericsDecorationTests : DecorationTestBase
{
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

    // Test if one is the service type is open generic and the implementation type is closed
    
    public interface IEvent;

    public sealed class MyEvent : IEvent;

    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        void Handle(TEvent @event);
    }

    public sealed class MyEventHandler : IEventHandler<MyEvent>
    {
        public int ReceivedCallsCount { get; private set; }
        
        public void Handle(MyEvent @event)
        {
            ReceivedCallsCount++;    
        }
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