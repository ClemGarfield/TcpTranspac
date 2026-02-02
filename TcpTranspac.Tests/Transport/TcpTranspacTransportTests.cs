using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TcpTranspac.Client;
using Xunit;

public class TcpTranspacTransportTests
{
    [Fact]
    public async Task Transport_can_connect_send_and_receive()
    {
        // Arrange
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var transport = new TcpTranspacTransport("127.0.0.1", port, NullLogger<TcpTranspacTransport>.Instance);

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

        var serverClient = await listener.AcceptTcpClientAsync();
        var serverStream = serverClient.GetStream();

        byte[] payload = { 1, 2, 3, 4 };
        await transport.SendAsync(payload);

        byte[] buffer = new byte[4];
        await serverStream.ReadExactlyAsync(buffer);

        // Assert send
        buffer.Should().Equal(payload);

        // Act receive
        await serverStream.WriteAsync(payload);

        await tcs.Task; // wait for BytesReceived

        // Assert receive
        received.Should().Equal(payload);

        // Cleanup
        await transport.DisconnectAsync();
        serverClient.Dispose();
        listener.Stop();
    }
}
