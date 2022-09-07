using FluentAssertions;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

public class MinMatchFinderTests
{
    [Test]
    public void Test1()
    {
        var w = new double[,]
        {
            {1, 2, 3},
            {5, 5, 6},
        };

        MinMatchFinder.FindMinMatch(w).Should().Equal(0, 1);
    }

    [Test]
    public void Test2()
    {
        var w = new double[,]
        {
            {2, 1, 1},
            {3, 1, 5},
            {2, 5, 6},
        };

        MinMatchFinder.FindMinMatch(w).Should().Equal(2, 1, 0);
    }
}
