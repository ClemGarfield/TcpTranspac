using Microsoft.Extensions.Logging;
using TcpTranspac.Client;
using TcpTranspac.Protocol;

class Program
{
    static async Task Main()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options => options.SingleLine = true);
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Program>();

        var router = new MessageRouter(loggerFactory.CreateLogger<MessageRouter>());
        router.Register(200, async msg =>
        {
            logger.LogInformation("Client received response: {Payload}", BitConverter.ToString(msg.Payload));
            await Task.CompletedTask;
        });

        var transport = new TcpTranspacTransport("127.0.0.1", 9001, loggerFactory.CreateLogger<TcpTranspacTransport>());

        var client = new TcpTranspacClient(
            transport,
            router,
            loggerFactory.CreateLogger<TcpTranspacClient>(),
            loggerFactory.CreateLogger<MessageEncoder>(),
            loggerFactory.CreateLogger<MessageDecoder>(),
            loggerFactory.CreateLogger<Framing>());

        await client.ConnectAsync();

        var message = new TcpTranspacMessage(
            Guid.NewGuid(),
            1,
            100,
            new byte[] { 1, 2, 3, 4 });

        logger.LogInformation("Sending message...");
        await client.SendAsync(message);

        Console.WriteLine("Press Enter to disconnect...");
        Console.ReadLine();

        await client.DisposeAsync();
    }
}