using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TcpTranspac.Server;
using Xunit;

public class TcpTranspacServerTransportTests
{
    [Fact]
    public async Task ServerTransport_can_receive_and_send()
    {
        // Arrange
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var transport = new TcpTranspacServerTransport(serverClient, NullLogger<TcpTranspacServerTransport>.Instance);

        byte[] received = Array.Empty<byte>();
        var tcs = new TaskCompletionSource();

        transport.BytesReceived += data =>
        {
            received = data.ToArray();
            tcs.SetResult();
            return Task.CompletedTask;
        };

        // Act
        await transport.ConnectAsync();

        var clientStream = client.GetStream();
        byte[] payload = { 9, 8, 7 };

        await clientStream.WriteAsync(payload);

        await tcs.Task;

        // Assert receive
        received.Should().Equal(payload);

        // Act send
        await transport.SendAsync(payload);

        byte[] buffer = new byte[3];
        await clientStream.ReadExactlyAsync(buffer);

        // Assert send
        buffer.Should().Equal(payload);

        // Cleanup
        await transport.DisconnectAsync();
        client.Dispose();
        listener.Stop();
    }
}