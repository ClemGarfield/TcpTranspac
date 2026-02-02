using Microsoft.Extensions.Logging;
using TcpTranspac.Protocol;
using TcpTranspac.Transport;

namespace TcpTranspac.Client
{
    public sealed class TcpTranspacClient : IAsyncDisposable
    {
        private readonly ITranspacTransport transport;
        private readonly MessageRouter router;
        private readonly ILogger<TcpTranspacClient>? logger;

        private readonly MessageEncoder encoder;
        private readonly MessageDecoder decoder;
        private readonly Framing framing;

        public bool IsConnected => transport.IsConnected;

        public TcpTranspacClient(
            ITranspacTransport transport,
            MessageRouter router,
            ILogger<TcpTranspacClient>? logger = null,
            ILogger<MessageEncoder>? encoderLogger = null,
            ILogger<MessageDecoder>? decoderLogger = null,
            ILogger<Framing>? framingLogger = null)
        {
            this.transport = transport;
            this.router = router;
            this.logger = logger;

            encoder = new MessageEncoder(encoderLogger);
            decoder = new MessageDecoder(decoderLogger);
            framing = new Framing(framingLogger);

            this.transport.BytesReceived += OnBytesReceived;

            logger?.LogInformation("TcpTranspacClient created");
        }

        public Task ConnectAsync()
        {
            logger?.LogInformation("Connecting client");
            return transport.ConnectAsync();
        }

        public Task DisconnectAsync()
        {
            logger?.LogInformation("Disconnecting client");
            return transport.DisconnectAsync();
        }

        public Task SendAsync(TcpTranspacMessage message)
        {
            var frame = encoder.Encode(message);

            logger?.LogDebug("Sending message type {Type} ({Length} bytes)",
                message.MessageType, frame.Length);

            return transport.SendAsync(frame);
        }

        private async Task OnBytesReceived(ReadOnlyMemory<byte> data)
        {
            logger?.LogDebug("Received {Length} bytes", data.Length);

            foreach (var frame in framing.ProcessIncomingData(data))
            {
                if (decoder.TryDecode(frame.Span, out var message, out var status))
                {
                    logger?.LogDebug("Decoded message type {Type}", message!.MessageType);
                    await router.Dispatch(message!);
                }
                else
                {
                    logger?.LogWarning("Decode error: {Status}", status);
                    await router.DispatchError(status);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            logger?.LogInformation("Disposing client");
            await transport.DisposeAsync();
        }
    }
}