using System;
using System.Threading.Tasks;

namespace TcpTranspac.Transport
{
    public interface ITranspacTransport : IAsyncDisposable
    {
        /// <summary>
        /// Indique si le transport est actuellement connecté.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Envoie une frame complète (déjà encodée par le protocole).
        /// </summary>
        Task SendAsync(ReadOnlyMemory<byte> frame);

        /// <summary>
        /// Événement déclenché lorsqu'un bloc d'octets est reçu.
        /// </summary>
        event Func<ReadOnlyMemory<byte>, Task>? BytesReceived;

        /// <summary>
        /// Établit la connexion (TCP, WebSocket, etc.).
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Ferme proprement la connexion.
        /// </summary>
        Task DisconnectAsync();
    }
}
