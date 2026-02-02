using System;
using System.Threading.Tasks;
using FluentAssertions;
using TcpTranspac.Protocol;
using Xunit;

public class MessageRouterTests
{
    [Fact]
    public async Task RouteAsync_calls_registered_handlers()
    {
        var router = new MessageRouter();

        ushort type = 100;
        int callCount = 0;

        router.Register(type, msg =>
        {
            callCount++;
            return Task.CompletedTask;
        });

        var message = new TcpTranspacMessage(Guid.NewGuid(), 1, type, Array.Empty<byte>());

        var handled = await router.RouteAsync(message);

        handled.Should().BeTrue();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task RouteAsync_returns_false_if_no_handler()
    {
        var router = new MessageRouter();
        var message = new TcpTranspacMessage(Guid.NewGuid(), 1, 999, Array.Empty<byte>());

        var handled = await router.RouteAsync(message);

        handled.Should().BeFalse();
    }
}
