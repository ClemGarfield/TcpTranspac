namespace TcpTranspac.Protocol
{
    /// <summary>
    /// Provides protocol-related constant values used for communication,
    /// including control characters, header size, protocol versioning,
    /// and payload/message constraints.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// ASCII Start of Text (STX) control character.
        /// </summary>
        public const byte STX = 0x02;

        /// <summary>
        /// ASCII End of Text (ETX) control character.
        /// </summary>
        public const byte ETX = 0x03;

        /// <summary>
        /// Fixed size of the binary header in bytes.
        /// </summary>
        public const int HeaderLength = 46;

        /// <summary>
        /// Specifies the offset, in bytes, at which the payload data begins within the buffer.
        /// </summary>
        /// <remarks>Use this constant when parsing or constructing buffers that contain a header
        /// preceding the payload. The value represents the number of bytes to skip to access the payload
        /// section.</remarks>
        public const int PayloadOffset = 27;

        /// <summary>
        /// Maximum allowed payload size in bytes.
        /// </summary>
        public const ushort MaxPayloadLength = ushort.MaxValue;

        /// <summary>
        /// Maximum allowable message identifier value.
        /// </summary>
        public const ulong MaxMessageId = ulong.MaxValue;

        /// <summary>
        /// Major version number of the protocol.
        /// </summary>
        public const byte VersionMajor = 1;

        /// <summary>
        /// Minor version number of the protocol.
        /// </summary>
        public const byte VersionMinor = 0;
    }
}