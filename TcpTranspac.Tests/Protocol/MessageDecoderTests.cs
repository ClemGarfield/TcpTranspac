using System;
using FluentAssertions;
using TcpTranspac.Protocol;
using Xunit;

public class MessageDecoderTests
{
    [Fact]
    public void Decode_invalid_crc_fails()
    {
        var encoder = new MessageEncoder();
        var decoder = new MessageDecoder();

        var msg = new TcpTranspacMessage(Guid.NewGuid(), 1, 10, new byte[] { 10, 20, 30 });
        var frame = encoder.Encode(msg);

        frame[5] ^= 0xFF; // corruption volontaire

        var ok = decoder.TryDecode(frame, out var decoded, out var status);

        ok.Should().BeFalse();
        decoded.Should().BeNull();
        status.Should().NotBe(DecodeStatus.Success);
    }
}
