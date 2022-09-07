using System.Collections.Generic;

namespace lib.Enhancers;

public static class Enhancer
{
    public static List<Move> Enhance(Screen problem, List<Move> moves)
    {
        return new CombinedEnhancer(new CutEnhancer(), new ColorEnhancer()).Enhance(problem, moves);
    }

    public static List<Move> Enhance2(Screen problem, List<Move> moves)
    {
        if (problem.InitialBlocks.Length == 1)
        {
            var first = new CombinedEnhancer(new ColorPicker(), new CutEnhancer());
            var second = new CombinedEnhancer(new CutEnhancer(), new ColorEnhancer());
            return new CombinedEnhancer(first, second).Enhance(problem, moves);
        }

        return Enhance(problem, moves);
    }
}
