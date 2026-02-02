using System;

namespace TcpTranspac.Protocol
{
    /// <summary>
    /// Represents the metadata header for a protocol message, containing identifiers, type information, versioning, and
    /// integrity checks.
    /// </summary>
    /// <remarks>The Header class encapsulates essential information required for message identification,
    /// tracing, and validation within the protocol. It includes fields for message and trace identifiers, message type,
    /// protocol version, payload length, and CRC values for both the header and payload to ensure data integrity. This
    /// type is intended for internal use and is not thread-safe.</remarks>
    internal sealed class Header
    {
        /// <summary>
        /// Gets the unique identifier for the message.
        /// </summary>
        public ulong MessageId { get; init; }
        /// <summary>
        /// Gets the unique identifier used to trace the operation or request across system boundaries.
        /// </summary>
        /// <remarks>Use this value to correlate logs, diagnostics, or events that belong to the same
        /// logical operation. The identifier remains constant for the lifetime of the operation.</remarks>
        public Guid TraceId { get; init; }
        /// <summary>
        /// Gets the type identifier for the message.
        /// </summary>
        public string MessageType { get; init; } = string.Empty;
        /// <summary>
        /// Gets the length of the payload, in bytes.
        /// </summary>
        public ushort PayloadLength { get; init; }
        /// <summary>
        /// Gets the major version number of the protocol or format.
        /// </summary>
        public byte VersionMajor { get; init; } = Constants.VersionMajor;
        /// <summary>
        /// Gets the minor version number of the protocol or format.
        /// </summary>
        public byte VersionMinor { get; init; } = Constants.VersionMinor;
        /// <summary>
        /// Gets the CRC-32 checksum value for the header data.
        /// </summary>
        public uint HeaderCrc { get; init; }
        /// <summary>
        /// Gets the CRC32 checksum of the payload data.
        /// </summary>
        public uint PayloadCrc { get; init; }

    }
}
