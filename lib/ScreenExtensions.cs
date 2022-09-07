using System.Linq;

namespace lib;

public static class ScreenExtensions
{
    public static int GetLineCutCost(this Screen screen) => GetLineCutCost(screen.InitialBlocks.OfType<PngBlock>().Any());
    public static int GetPointCutCost(this Screen screen) => GetPointCutCost(screen.InitialBlocks.OfType<PngBlock>().Any());
    public static int GetLineCutCost(bool withSourcePng) => withSourcePng ? 2 : 7;
    public static int GetPointCutCost(bool withSourcePng) => withSourcePng ? 3 : 10;
    public static int GetColorCost() => 5;
    public static int GetSwapCost() => 3;
    public static int GetMergeCost() => 1;

}
