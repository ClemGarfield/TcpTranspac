using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


namespace TcpTranspac.Protocol
{
    public sealed class Framing
    {
        private readonly List<byte> buffer = new();
        private readonly ILogger<Framing>? logger;

        public Framing(ILogger<Framing>? logger = null)
        {
            this.logger = logger;
        }

        public IEnumerable<ReadOnlyMemory<byte>> ProcessIncomingData(ReadOnlyMemory<byte> data)
        {
            logger?.LogDebug("Framing received {Length} bytes", data.Length);

            // Ajout des octets reçus dans le buffer interne
            buffer.AddRange(data.Span.ToArray());

            List<ReadOnlyMemory<byte>> messages = new();

            while (true)
            {
                // 1) Chercher STX
                int start = buffer.IndexOf(Constants.STX);
                if (start < 0)
                {
                    logger?.LogDebug("No STX found in buffer (size {Size})", buffer.Count);
                    break;
                }

                // Supprimer tout ce qui est avant STX
                if (start > 0)
                {
                    logger?.LogDebug("Discarding {Count} bytes before STX", start);
                    buffer.RemoveRange(0, start);
                }

                // 2) Vérifier qu'on a au moins le header complet
                if (buffer.Count < 1 + Constants.HeaderLength)
                {
                    logger?.LogDebug("Incomplete header: have {Have}, need {Need}",
                        buffer.Count, 1 + Constants.HeaderLength);
                    break;
                }

                // 3) Lire la longueur du payload (dans le header)

                Span<byte> span = CollectionsMarshal.AsSpan(buffer);

                if (buffer.Count < Constants.PayloadOffset + 2)
                {
                    logger?.LogDebug("Not enough data to read payload length");
                    break;
                }

                ushort payloadLength = BinaryPrimitives.ReadUInt16BigEndian(
                    span.Slice(Constants.PayloadOffset, 2));


                int totalLength =
                    1 +                         // STX
                    Constants.HeaderLength +    // Header (46 bytes)
                    payloadLength +
                    1 +                         // ETX
                    8;                          // CRC64 final

                // 4) Vérifier qu'on a toute la trame
                if (buffer.Count < totalLength)
                {
                    logger?.LogDebug("Incomplete frame: have {Have}, need {Need}",
                        buffer.Count, totalLength);
                    break;
                }

                // 5) Vérifier ETX
                int etxIndex = 1 + Constants.HeaderLength + payloadLength;
                if (buffer[etxIndex] != Constants.ETX)
                {
                    logger?.LogWarning("Invalid ETX at index {Index}, discarding 1 byte", etxIndex);
                    buffer.RemoveAt(0);
                    continue;
                }

                // 6) Extraire la trame complète
                byte[] frame = buffer.Take(totalLength).ToArray();
                messages.Add(frame);

                logger?.LogDebug("Extracted complete frame of {Length} bytes", totalLength);

                // 7) Supprimer la trame du buffer
                buffer.RemoveRange(0, totalLength);
            }

            return messages;
        }
    }
}