using System;
using System.Buffers.Binary;
using System.IO.Hashing;
using Microsoft.Extensions.Logging;

namespace TcpTranspac.Protocol
{
    public sealed class MessageEncoder
    {
        private readonly ILogger<MessageEncoder>? logger;

        public MessageEncoder(ILogger<MessageEncoder>? logger = null)
        {
            this.logger = logger;
        }

        public byte[] Encode(TcpTranspacMessage message)
        {
            if (message.Payload.Length > Constants.MaxPayloadLength)
            {
                logger?.LogWarning(
                    "Payload too large: {Length} bytes (max {Max})",
                    message.Payload.Length,
                    Constants.MaxPayloadLength);

                throw new ArgumentException("Payload too large.", nameof(message));
            }

            logger?.LogDebug(
                "Encoding message type {Type}, payload {PayloadLength} bytes",
                message.MessageType,
                message.Payload.Length);

            byte[] header = new byte[Constants.HeaderLength];


            // 1) MessageId (8 bytes)
            BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(0, 8), message.MessageId);

            // 2) TraceId (16 bytes)
            message.TraceId.TryWriteBytes(header.AsSpan(8, 16));

            // 3) MessageType (2 bytes)
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(24, 2), message.MessageType);

            // 4) PayloadLength (2 bytes)
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(26, 2), (ushort)message.Payload.Length);

            // 5) Version
            header[28] = Constants.VersionMajor;
            header[29] = Constants.VersionMinor;

            // 6) CRC64 du header (sans CRC)
            ulong headerCrc = Crc64.HashToUInt64(header.AsSpan(0, 30));
            BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(30, 8), headerCrc);

            // 7) CRC64 du payload
            ulong payloadCrc = Crc64.HashToUInt64(message.Payload);
            BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(38, 8), payloadCrc);

            // 8) Assemblage final
            byte[] frame = new byte[
                1 +                     // STX
                header.Length +
                message.Payload.Length +
                1 +                     // ETX
                8                       // CRC64 final
            ];

            int offset = 0;

            frame[offset++] = Constants.STX;
            Buffer.BlockCopy(header, 0, frame, offset, header.Length);
            offset += header.Length;

            Buffer.BlockCopy(message.Payload, 0, frame, offset, message.Payload.Length);
            offset += message.Payload.Length;

            frame[offset++] = Constants.ETX;

            // CRC64 final = CRC(header + payload)
            ulong finalCrc = Crc64.HashToUInt64(frame.AsSpan(1, header.Length + message.Payload.Length));
            BinaryPrimitives.WriteUInt64BigEndian(frame.AsSpan(offset, 8), finalCrc);

            logger?.LogDebug(
                "Encoded frame: header CRC {HeaderCrc:X16}, payload CRC {PayloadCrc:X16}, final CRC {FinalCrc:X16}",
                headerCrc, payloadCrc, finalCrc);

            return frame;
        }
    }
}