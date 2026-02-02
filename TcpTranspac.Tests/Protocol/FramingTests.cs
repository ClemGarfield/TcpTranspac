using FluentAssertions;
using TcpTranspac.Protocol;

public class FramingTests
{
    [Fact]
    public void Single_frame_in_one_chunk()
    {
        var framing = new Framing();
        var encoder = new MessageEncoder();

        var msg = new TcpTranspacMessage(Guid.NewGuid(), 1, 10, new byte[] { 1, 2, 3 });
        var frame = encoder.Encode(msg);

        var frames = framing.ProcessIncomingData(frame).ToList();

        frames.Should().HaveCount(1);
        frames[0].ToArray().Should().Equal(frame);
    }

    [Fact]
    public void Single_frame_in_multiple_chunks()
    {
        var framing = new Framing();
        var encoder = new MessageEncoder();

        var msg = new TcpTranspacMessage(Guid.NewGuid(), 1, 10, new byte[] { 1, 2, 3, 4, 5 });
        var frame = encoder.Encode(msg);

        var part1 = frame[..5];
        var part2 = frame[5..];

        framing.ProcessIncomingData(part1).Should().BeEmpty();

        var frames = framing.ProcessIncomingData(part2).ToList();

        frames.Should().HaveCount(1);
        frames[0].ToArray().Should().Equal(frame);
    }
}
