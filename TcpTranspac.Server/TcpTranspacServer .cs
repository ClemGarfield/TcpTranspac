using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using TcpTranspac.Client;
using TcpTranspac.Protocol;

namespace TcpTranspac.Server
{
    public sealed class TcpTranspacServer : IAsyncDisposable
    {
        private readonly int port;
        private readonly MessageRouter router;
        private readonly ILogger<TcpTranspacServer>? logger;
        private readonly ILoggerFactory? loggerFactory;

        private TcpListener? listener;
        private readonly List<TcpTranspacClient> clients = new();

        public TcpTranspacServer(
            int port,
            MessageRouter router,
            ILogger<TcpTranspacServer>? logger = null,
            ILoggerFactory? loggerFactory = null)
        {
            this.port = port;
            this.router = router;
            this.logger = logger;
            this.loggerFactory = loggerFactory;
        }

        public Task StartAsync()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            logger?.LogInformation("TcpTranspacServer listening on port {Port}", port);

            _ = Task.Run(AcceptLoop);

            return Task.CompletedTask;
        }

        private async Task AcceptLoop()
        {
            logger?.LogInformation("Accept loop started");

            while (true)
            {
                TcpClient tcpClient = await listener!.AcceptTcpClientAsync();

                logger?.LogInformation("Accepted connection from {Remote}", tcpClient.Client.RemoteEndPoint);

                var transportLogger = loggerFactory?.CreateLogger<TcpTranspacServerTransport>();
                var clientLogger = loggerFactory?.CreateLogger<TcpTranspacClient>();
                var encoderLogger = loggerFactory?.CreateLogger<MessageEncoder>();
                var decoderLogger = loggerFactory?.CreateLogger<MessageDecoder>();
                var framingLogger = loggerFactory?.CreateLogger<Framing>();

                var transport = new TcpTranspacServerTransport(tcpClient, transportLogger);

                var client = new TcpTranspacClient(
                    transport,
                    router,
                    clientLogger,
                    encoderLogger,
                    decoderLogger,
                    framingLogger);

                clients.Add(client);

                await transport.ConnectAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            logger?.LogInformation("Shutting down TcpTranspacServer");

            foreach (var c in clients)
                await c.DisposeAsync();

            listener?.Stop();
        }
    }
}