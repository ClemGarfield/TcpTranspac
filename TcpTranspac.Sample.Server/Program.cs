using Microsoft.Extensions.Logging;
using TcpTranspac.Protocol;
using TcpTranspac.Server;

class Program
{
    static async Task Main()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

        MessageRouter router = new MessageRouter(loggerFactory.CreateLogger<MessageRouter>());
        router.Register(100, async msg =>
        {
            logger.LogInformation("Received message {Id} type {Type} payload: {Payload}",
                msg.MessageId, msg.MessageType, BitConverter.ToString(msg.Payload));
            await Task.CompletedTask;
        });

        var server = new TcpTranspacServer(
            port: 9001,
            router: router,
            logger: loggerFactory.CreateLogger<TcpTranspacServer>(),
            loggerFactory: loggerFactory);

        logger.LogInformation("Starting TcpTranspac server on port 9001...");
        await server.StartAsync();

        Console.WriteLine("Press Enter to stop...");
        Console.ReadLine();

        await server.DisposeAsync();
    }
}