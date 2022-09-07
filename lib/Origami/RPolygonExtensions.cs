using System.Collections.Generic;
using System.Linq;

namespace lib.Origami
{
    public static class RPolygonExtensions
    {
        public static bool HasSegment(this RPolygon polygon, RSeg segment)
        {
            return polygon.Segments.Any(segment.Equals);
        }

        public static IEnumerable<RSeg> GetCommonSegments(this RPolygon polygon, RPolygon thatPolygon)
        {
            foreach (var thisSegment in polygon.Segments)
            {
                foreach (var thatSegment in thatPolygon.Segments)
                {
                    if (thisSegment.Equals(thatSegment))
                        yield return thisSegment;
                }
            }
        }
    }
}
