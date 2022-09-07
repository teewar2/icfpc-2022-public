using FluentAssertions;
using lib;
using lib.Algorithms;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;

namespace tests;

[TestFixture]
public class ScreenTests
{
    [Test]
    public void TestLoad()
    {
        var fn = FileHelper.FindFilenameUpwards("problems/problem1.png");
        using var image = (Image<Rgba32>)Image.Load(fn, new PngDecoder());
        var screen = Screen.LoadFrom(image);
    }

    [Test]
    public void TestLoadProblem()
    {
        var problem = Screen.LoadProblem(1);
    }

    [Test]
    public void TestDiffTo()
    {
        var screen1 = Screen.LoadProblem(1);
        var screen2 = Screen.LoadProblem(1);
        screen1.DiffTo(screen2).Should().Be(0);
    }

    [Test]
    public void TestDiffTo2()
    {
        var screen1 = Screen.LoadProblem(1);
        var screen2 = Screen.LoadProblem(1);
        screen1.Pixels[0, 0] = new Rgba(255, 0, 0, 0);
        screen2.Pixels[0, 0] = new Rgba(0, 0, 0, 0);
        screen1.DiffTo(screen2).Should().Be(255 * 0.05);
    }

}
