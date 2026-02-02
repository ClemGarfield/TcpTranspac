using Microsoft.Extensions.Logging;

namespace TcpTranspac.Protocol
{
    public sealed class MessageRouter
    {
        private readonly Dictionary<ushort, List<Func<TcpTranspacMessage, Task>>> handlers = new();
        private readonly ILogger<MessageRouter>? logger;

        public MessageRouter(ILogger<MessageRouter>? logger = null)
        {
            this.logger = logger;
        }

        public void Register(ushort messageType, Func<TcpTranspacMessage, Task> handler)
        {
            if (!handlers.TryGetValue(messageType, out var list))
            {
                list = new List<Func<TcpTranspacMessage, Task>>();
                handlers[messageType] = list;
            }

            list.Add(handler);

            logger?.LogInformation(
                "Registered handler for message type {MessageType}. Total handlers: {Count}",
                messageType, list.Count);
        }

        /// <summary>
        /// Méthode standard appelée par TcpTranspacClient.
        /// </summary>
        public Task<bool> Dispatch(TcpTranspacMessage message)
            => RouteAsync(message);

        /// <summary>
        /// Méthode appelée en cas d’erreur de décodage.
        /// </summary>
        public Task DispatchError(DecodeStatus status)
        {
            logger?.LogWarning("Decode error: {Status}", status);
            return Task.CompletedTask;
        }

        public async Task<bool> RouteAsync(TcpTranspacMessage message)
        {
            if (!handlers.TryGetValue(message.MessageType, out var list) || list.Count == 0)
            {
                logger?.LogWarning(
                    "No handler registered for message type {MessageType}",
                    message.MessageType);

                return false;
            }

            logger?.LogDebug(
                "Dispatching message type {MessageType} to {Count} handler(s)",
                message.MessageType, list.Count);

            foreach (var handler in list)
            {
                try
                {
                    await handler(message);
                }
                catch (Exception ex)
                {
                    logger?.LogError(
                        ex,
                        "Handler error for message type {MessageType}",
                        message.MessageType);
                }
            }

            return true;
        }

        public async Task<bool> RouteParallelAsync(TcpTranspacMessage message)
        {
            if (!handlers.TryGetValue(message.MessageType, out var list) || list.Count == 0)
            {
                logger?.LogWarning(
                    "No handler registered for message type {MessageType}",
                    message.MessageType);

                return false;
            }

            logger?.LogDebug(
                "Dispatching message type {MessageType} in parallel to {Count} handler(s)",
                message.MessageType, list.Count);

            var tasks = new List<Task>(list.Count);

            foreach (var handler in list)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await handler(message);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(
                            ex,
                            "Parallel handler error for message type {MessageType}",
                            message.MessageType);
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return true;
        }
    }
}