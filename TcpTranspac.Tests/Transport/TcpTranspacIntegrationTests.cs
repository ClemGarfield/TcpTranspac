using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TcpTranspac.Client;
using TcpTranspac.Protocol;
using TcpTranspac.Server;
using Xunit;

public class TcpTranspacIntegrationTests
{
    [Fact]
    public async Task Client_and_server_exchange_messages_successfully()
    {
        // Arrange
        var router = new MessageRouter(NullLogger<MessageRouter>.Instance);

        TcpTranspacMessage? received = null;
        var tcs = new TaskCompletionSource();

        router.Register(100, msg =>
        {
            received = msg;
            tcs.SetResult();
            return Task.CompletedTask;
        });

        var server = new TcpTranspacServer(
            port: 9001,
            router: router,
            logger: NullLogger<TcpTranspacServer>.Instance,
            loggerFactory: NullLoggerFactory.Instance);

        await server.StartAsync();

        var clientTransport = new TcpTranspacTransport("127.0.0.1", 9001, NullLogger<TcpTranspacTransport>.Instance);

        var client = new TcpTranspacClient(
            clientTransport,
            router,
            NullLogger<TcpTranspacClient>.Instance,
            NullLogger<MessageEncoder>.Instance,
            NullLogger<MessageDecoder>.Instance,
            NullLogger<Framing>.Instance);

        await client.ConnectAsync();

        var message = new TcpTranspacMessage(Guid.NewGuid(), 1, 100, new byte[] { 1, 2, 3 });

        // Act
        await client.SendAsync(message);

        await tcs.Task;

        // Assert
        received.Should().NotBeNull();
        received!.MessageType.Should().Be(100);
        received.Payload.Should().Equal(new byte[] { 1, 2, 3 });

        // Cleanup
        await client.DisposeAsync();
        await server.DisposeAsync();
    }
}