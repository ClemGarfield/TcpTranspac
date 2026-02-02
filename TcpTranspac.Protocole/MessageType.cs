namespace TcpTranspac.Protocol
{
    /// <summary>
    /// Defines the binary identifiers for all supported message types.
    /// </summary>
    internal static class MessageTypes
    {
        // Data plane
        public const ushort DATA = 0x01;
        public const ushort ACK = 0x02;
        public const ushort NACK = 0x03;

        // Control plane
        public const ushort HEARTBEAT = 0x10;
        public const ushort HELLO = 0x11;
        public const ushort GOODBYE = 0x12;
        public const ushort PING = 0x13;
        public const ushort PONG = 0x14;

        // Error plane
        public const ushort ERROR = 0x20;
        public const ushort INVALID_HEADER = 0x21;
        public const ushort INVALID_PAYLOAD = 0x22;
        public const ushort CRC_ERROR = 0x23;
        public const ushort UNSUPPORTED_TYPE = 0x24;
        public const ushort VERSION_MISMATCH = 0x25;

        // Application plane
        public const ushort COMMAND = 0x30;
        public const ushort EVENT = 0x31;
        public const ushort STATUS = 0x32;
        public const ushort LOG = 0x33;
        public const ushort METRICS = 0x34;

        // System plane
        public const ushort CONFIG = 0x40;
        public const ushort SYNC = 0x41;
        public const ushort AUTH = 0x42;
        public const ushort SESSION = 0x43;

        // Reserved for future use
        public const ushort RESERVED_1 = 0xF0;
        public const ushort RESERVED_2 = 0xF1;
        public const ushort RESERVED_3 = 0xF2;
    }
}