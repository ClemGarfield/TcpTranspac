using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using TcpTranspac.Transport;

namespace TcpTranspac.Server
{
    public sealed class TcpTranspacServerTransport : ITranspacTransport
    {
        private readonly TcpClient client;
        private readonly ILogger<TcpTranspacServerTransport>? logger;

        private NetworkStream? stream;
        private CancellationTokenSource? cts;

        public bool IsConnected => client.Connected;

        public event Func<ReadOnlyMemory<byte>, Task>? BytesReceived;

        public TcpTranspacServerTransport(TcpClient client, ILogger<TcpTranspacServerTransport>? logger = null)
        {
            this.client = client;
            this.logger = logger;
        }

        public Task ConnectAsync()
        {
            logger?.LogInformation("Initializing server-side transport for {Remote}", client.Client.RemoteEndPoint);

            stream = client.GetStream();
            cts = new CancellationTokenSource();

            _ = Task.Run(() => ReceiveLoop(cts.Token));

            return Task.CompletedTask;
        }

        public async Task DisconnectAsync()
        {
            logger?.LogInformation("Disconnecting client {Remote}", client.Client.RemoteEndPoint);

            try
            {
                cts?.Cancel();
                stream?.Dispose();
                client?.Dispose();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Error during server-side disconnect");
            }

            await Task.CompletedTask;
        }

        public async Task SendAsync(ReadOnlyMemory<byte> frame)
        {
            if (stream == null)
                throw new InvalidOperationException("Transport not connected");

            logger?.LogDebug("Sending {Length} bytes to {Remote}", frame.Length, client.Client.RemoteEndPoint);

            await stream.WriteAsync(frame).ConfigureAwait(false);
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            logger?.LogInformation("Receive loop started for {Remote}", client.Client.RemoteEndPoint);

            while (!token.IsCancellationRequested)
            {
                int read;
                try
                {
                    read = await stream!.ReadAsync(buffer, token);
                    if (read == 0)
                    {
                        logger?.LogInformation("Client {Remote} closed connection", client.Client.RemoteEndPoint);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Receive loop error for {Remote}", client.Client.RemoteEndPoint);
                    break;
                }

                logger?.LogDebug("Received {Length} bytes from {Remote}", read, client.Client.RemoteEndPoint);

                var slice = new ReadOnlyMemory<byte>(buffer, 0, read);
                if (BytesReceived != null)
                    await BytesReceived(slice);
            }

            logger?.LogInformation("Receive loop ended for {Remote}", client.Client.RemoteEndPoint);
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
        }
    }
}