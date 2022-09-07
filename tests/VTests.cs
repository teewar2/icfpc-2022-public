using FluentAssertions;
using lib;
using NUnit.Framework;

namespace tests;

[TestFixture]
public class VTests
{
    [TestCase("-1 -1", "0 0", "180", "1 1")]
    [TestCase("9 9", "10 10", "180", "11 11")]
    [TestCase("0 1", "0 0", "90", "1 0")]
    [TestCase("0 1", "0 0", "180", "0 -1")]
    [TestCase("0 1", "0 0", "0", "0 1")]
    [TestCase("0 1", "0 0", "45", "1 1")] // Because of rounding
    public void V_Should_BeRotated(string point, string pivot, string x, string expectedPoint)
    {
        V p = point;
        V pv = pivot;
        var angle = int.Parse(x);
        V expected = expectedPoint;
        p.Rotate(pv, angle).Should().Be(expected);
        expected.Rotate(pv, -angle).Should().Be(p);
    }

    [TestCase("0 0", "-1 1", "1 1", "0 2")]
    [TestCase("1 1", "0 2", "2 2", "1 3")]
    [TestCase("1 1", "4 2", "5 2", "1 3")]
    [TestCase("1 1", "0 0", "0 1", "-1 1")]
    [TestCase("1 1", "0 1", "0 0", "-1 1")]
    [TestCase("0 0", "1 1", "0 2", "2 2")]
    public void V_Should_BeReflected(string point, string start, string finish, string expectedPoint)
    {
        V p = point;
        V startPoint = start;
        V finishPoint = finish;
        V expected = expectedPoint;
        p.Reflect(startPoint, finishPoint).Should().Be(expected);
        expected.Reflect(startPoint, finishPoint).Should().Be(p);
    }
}
