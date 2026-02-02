using System;

namespace TcpTranspac.Protocol
{
    /// <summary>
    /// Represents a TCP Transpac protocol message, including identifiers,
    /// message type, and raw binary payload.
    /// </summary>
    public sealed class TcpTranspacMessage
    {
        public Guid TraceId { get; }
        public ulong MessageId { get; }
        public ushort MessageType { get; }
        public byte[] Payload { get; }

        public TcpTranspacMessage(Guid traceId, ulong messageId, ushort messageType, byte[] payload)
        {
            TraceId = traceId;
            MessageId = messageId;
            MessageType = messageType;
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }
    }
}