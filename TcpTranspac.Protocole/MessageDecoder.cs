using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.IO.Hashing;

namespace TcpTranspac.Protocol
{
    public sealed class MessageDecoder
    {
        private readonly ILogger<MessageDecoder>? logger;

        public MessageDecoder(ILogger<MessageDecoder>? logger = null)
        {
            this.logger = logger;
        }

        public TcpTranspacMessage Decode(ReadOnlySpan<byte> frame)
        {
            if (!TryDecode(frame, out var message, out var status))
            {
                logger?.LogWarning("Decode failed: {Status}", status);
                throw new InvalidOperationException(status.ToString());
            }

            return message!;
        }

        public bool TryDecode(
            ReadOnlySpan<byte> frame,
            out TcpTranspacMessage? message,
            out DecodeStatus status)
        {
            message = null;
            status = DecodeStatus.Success;

            logger?.LogDebug("Decoding frame of {Length} bytes", frame.Length);

            if (frame.Length < 1 + Constants.HeaderLength + 1 + 8)
            {
                status = DecodeStatus.InvalidLength.WithDetails(
                    $"Frame too short: {frame.Length} bytes");

                logger?.LogWarning("Invalid frame length: {Length}", frame.Length);
                return false;
            }

            int offset = 0;

            // 1) STX
            if (frame[offset++] != Constants.STX)
            {
                status = DecodeStatus.InvalidStartDelimiter;
                logger?.LogWarning("Invalid STX delimiter");
                return false;
            }

            // 2) Header
            ReadOnlySpan<byte> headerSpan = frame.Slice(offset, Constants.HeaderLength);
            offset += Constants.HeaderLength;

            // 3) Payload length
            ushort payloadLength = BinaryPrimitives.ReadUInt16BigEndian(headerSpan.Slice(26, 2));

            int expectedLength =
                1 +
                Constants.HeaderLength +
                payloadLength +
                1 +
                8;

            if (frame.Length != expectedLength)
            {
                status = DecodeStatus.InvalidLength.WithDetails(
                    $"Expected {expectedLength} bytes, got {frame.Length}");

                logger?.LogWarning("Frame length mismatch: expected {Expected}, got {Actual}",
                    expectedLength, frame.Length);

                return false;
            }

            // 4) Payload
            ReadOnlySpan<byte> payloadSpan = frame.Slice(offset, payloadLength);
            offset += payloadLength;

            // 5) ETX
            if (frame[offset++] != Constants.ETX)
            {
                status = DecodeStatus.InvalidEndDelimiter;
                logger?.LogWarning("Invalid ETX delimiter");
                return false;
            }

            // 6) CRC64 final
            ulong finalCrc = BinaryPrimitives.ReadUInt64BigEndian(frame.Slice(offset, 8));
            ulong computedFinalCrc = Crc64.HashToUInt64(frame.Slice(1, Constants.HeaderLength + payloadLength));

            if (computedFinalCrc != finalCrc)
            {
                status = DecodeStatus.FrameCrcMismatch.WithDetails(
                    $"Expected {finalCrc:X16}, computed {computedFinalCrc:X16}");

                logger?.LogWarning("Final CRC mismatch: expected {Expected}, computed {Computed}",
                    finalCrc, computedFinalCrc);

                return false;
            }

            // 7) CRC64 header
            ulong headerCrc = BinaryPrimitives.ReadUInt64BigEndian(headerSpan.Slice(30, 8));
            ulong computedHeaderCrc = Crc64.HashToUInt64(headerSpan.Slice(0, 30));

            if (computedHeaderCrc != headerCrc)
            {
                status = DecodeStatus.HeaderCrcMismatch.WithDetails(
                    $"Expected {headerCrc:X16}, computed {computedHeaderCrc:X16}");

                logger?.LogWarning("Header CRC mismatch: expected {Expected}, computed {Computed}",
                    headerCrc, computedHeaderCrc);

                return false;
            }

            // 8) CRC64 payload
            ulong payloadCrc = BinaryPrimitives.ReadUInt64BigEndian(headerSpan.Slice(38, 8));
            ulong computedPayloadCrc = Crc64.HashToUInt64(payloadSpan);

            if (computedPayloadCrc != payloadCrc)
            {
                status = DecodeStatus.PayloadCrcMismatch.WithDetails(
                    $"Expected {payloadCrc:X16}, computed {computedPayloadCrc:X16}");

                logger?.LogWarning("Payload CRC mismatch: expected {Expected}, computed {Computed}",
                    payloadCrc, computedPayloadCrc);

                return false;
            }

            // 9) Reconstruction du message
            ulong messageId = BinaryPrimitives.ReadUInt64BigEndian(headerSpan.Slice(0, 8));

            Span<byte> guidBytes = stackalloc byte[16];
            headerSpan.Slice(8, 16).CopyTo(guidBytes);
            Guid traceId = new Guid(guidBytes);

            ushort messageType = BinaryPrimitives.ReadUInt16BigEndian(headerSpan.Slice(24, 2));

            byte[] payload = payloadSpan.ToArray();

            message = new TcpTranspacMessage(traceId, messageId, messageType, payload);

            logger?.LogDebug("Decoded message type {Type}, payload {PayloadLength} bytes",
                messageType, payloadLength);

            return true;
        }
    }
}