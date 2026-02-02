using System;
using FluentAssertions;
using TcpTranspac.Protocol;
using Xunit;

public class MessageEncoderTests
{
    [Fact]
    public void EncodeDecode_roundtrip_ok()
    {
        var encoder = new MessageEncoder();
        var decoder = new MessageDecoder();

        var msg = new TcpTranspacMessage(
            Guid.NewGuid(),
            42,
            1001,
            new byte[] { 1, 2, 3, 4 });

        var frame = encoder.Encode(msg);

        var ok = decoder.TryDecode(frame, out var decoded, out var status);

        ok.Should().BeTrue();
        status.Should().Be(DecodeStatus.Success);
        decoded.Should().NotBeNull();
        decoded!.MessageId.Should().Be(msg.MessageId);
        decoded.MessageType.Should().Be(msg.MessageType);
        decoded.Payload.Should().Equal(msg.Payload);
    }
}
