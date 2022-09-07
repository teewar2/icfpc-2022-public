using System.Collections.Generic;

namespace lib.Enhancers;

public interface ISolutionEnhancer
{
    List<Move> Enhance(Screen problem, List<Move> moves);
}
