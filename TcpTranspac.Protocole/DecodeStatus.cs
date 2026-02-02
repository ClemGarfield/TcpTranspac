namespace TcpTranspac.Protocol
{
    public sealed class DecodeStatus
    {
        public string Name { get; }
        public string? Details { get; }
        public bool IsRecoverable { get; }
        public bool ShouldRetry { get; }

        private DecodeStatus(string name, bool isRecoverable = false, bool shouldRetry = false, string? details = null)
        {
            Name = name;
            Details = details;
            IsRecoverable = isRecoverable;
            ShouldRetry = shouldRetry;
        }

        public DecodeStatus WithDetails(string details)
            => new(Name, IsRecoverable, ShouldRetry, details);

        public override string ToString()
            => Details is null ? Name : $"{Name}: {Details}";

        // --- Statuts connus du protocole ---

        public static readonly DecodeStatus Success =
            new("Success");

        public static readonly DecodeStatus InvalidLength =
            new("InvalidLength", isRecoverable: false, shouldRetry: false);

        public static readonly DecodeStatus InvalidStartDelimiter =
            new("InvalidStartDelimiter", isRecoverable: false, shouldRetry: false);

        public static readonly DecodeStatus InvalidEndDelimiter =
            new("InvalidEndDelimiter", isRecoverable: false, shouldRetry: false);

        public static readonly DecodeStatus HeaderCrcMismatch =
            new("HeaderCrcMismatch", isRecoverable: true, shouldRetry: true);

        public static readonly DecodeStatus PayloadCrcMismatch =
            new("PayloadCrcMismatch", isRecoverable: true, shouldRetry: true);

        public static readonly DecodeStatus FrameCrcMismatch =
            new("FrameCrcMismatch", isRecoverable: true, shouldRetry: true);
    }
}