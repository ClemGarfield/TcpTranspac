using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using TcpTranspac.Transport;

namespace TcpTranspac.Client
{
    public sealed class TcpTranspacTransport : ITranspacTransport
    {
        private readonly string host;
        private readonly int port;
        private readonly ILogger<TcpTranspacTransport>? logger;

        private TcpClient? client;
        private NetworkStream? stream;
        private CancellationTokenSource? cts;

        public bool IsConnected => client?.Connected ?? false;

        public event Func<ReadOnlyMemory<byte>, Task>? BytesReceived;

        public TcpTranspacTransport(string host, int port, ILogger<TcpTranspacTransport>? logger = null)
        {
            this.host = host;
            this.port = port;
            this.logger = logger;
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected");

            logger?.LogInformation("Connecting to {Host}:{Port}", host, port);

            client = new TcpClient();
            await client.ConnectAsync(host, port);

            stream = client.GetStream();
            cts = new CancellationTokenSource();

            logger?.LogInformation("Connected to {Host}:{Port}", host, port);

            _ = Task.Run(() => ReceiveLoop(cts.Token));
        }

        public async Task DisconnectAsync()
        {
            logger?.LogInformation("Disconnecting from {Host}:{Port}", host, port);

            try
            {
                cts?.Cancel();
                stream?.Dispose();
                client?.Dispose();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Error during disconnect");
            }

            await Task.CompletedTask;
        }

        public async Task SendAsync(ReadOnlyMemory<byte> frame)
        {
            if (stream == null)
                throw new InvalidOperationException("Transport not connected");

            logger?.LogDebug("Sending {Length} bytes", frame.Length);

            await stream.WriteAsync(frame).ConfigureAwait(false);
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            logger?.LogInformation("Receive loop started");

            while (!token.IsCancellationRequested)
            {
                int read;
                try
                {
                    read = await stream!.ReadAsync(buffer, token);
                    if (read == 0)
                    {
                        logger?.LogInformation("Remote closed connection");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Receive loop error");
                    break;
                }

                logger?.LogDebug("Received {Length} bytes", read);

                var slice = new ReadOnlyMemory<byte>(buffer, 0, read);
                if (BytesReceived != null)
                    await BytesReceived(slice);
            }

            logger?.LogInformation("Receive loop ended");
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
        }
    }
}