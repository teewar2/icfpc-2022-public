using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace lib.Origami
{
	public static class GeometryExtensions
	{
		public static VR GetProjectionOntoLine(this VR point, RSeg line)
		{
			var lineVector = line.ToVector();
			var perp = new VR(-lineVector.Y, lineVector.X);
			var projectionOntoLine = new RSeg(point, point + perp).GetLineIntersectionWithLine(line)!.Value;
			return projectionOntoLine;
		}

		public static VR? GetIntersectionWithLine(this RSeg segment, RSeg line)
		{
			var point = GetLineIntersectionWithLine(segment, line);
			if (point.HasValue)
			{
				if (IsBetween(segment.Start.X, point.Value.X, segment.End.X) && IsBetween(segment.Start.Y, point.Value.Y, segment.End.Y))
					return point;
			}
			return null;
		}

		public static VR? GetLineIntersectionWithLine(this RSeg thisLine, RSeg otherLine)
		{
			var A1 = thisLine.End - thisLine.Start;
			var B1 = thisLine.Start;

			var A2 = otherLine.End - otherLine.Start;
			var B2 = otherLine.Start;

			var denominator = A1.Y*A2.X - A2.Y*A1.X;

			if (denominator == 0)
				return null;

			var t2 = ((B2.Y - B1.Y) * A1.X + (B1.X - B2.X) * A1.Y) / denominator;
			return A2 * t2 + B2;
		}

		public static bool AreSegmentsOnSameLine(this RSeg segment1, RSeg segment2)
		{
			var A1 = segment1.End - segment1.Start;
			var B1 = segment1.Start;

			var A2 = segment2.End - segment2.Start;
			var B2 = segment2.Start;

			var denominator = A1.Y * A2.X - A2.Y * A1.X;
			if (denominator != 0)
				return false;

			if (A1.X == 0 && A2.X == 0)
				return B1.X == B2.X;

			if (A1.Y == 0 && A2.Y == 0)
				return B1.Y == B2.Y;

			return B2.X == (B1.Y - B2.Y)/A1.Y*A1.X + B1.X;
		}

		public static VR? GetIntersection(this RSeg segment, RSeg intersector)
		{
			var point = segment.GetIntersectionWithLine(intersector);
			if (point.HasValue)
			{
				if (IsBetween(intersector.Start.X, point.Value.X, intersector.End.X) && IsBetween(intersector.Start.Y, point.Value.Y, intersector.End.Y))
					return point;
			}
			return null;
		}

        public static IEnumerable<VR> GetIntersectionWithEdge(this RSeg segment, RSeg edge)
		{
			var point = segment.GetIntersectionWithLine(edge);
			if (point.HasValue)
			{
				if (IsBetween(edge.Start.X, point.Value.X, edge.End.X) && IsBetween(edge.Start.Y, point.Value.Y, edge.End.Y))
					return new[]{point.Value};
                return Array.Empty<VR>();
            }

            var startClassification = segment.Start.Classify(edge);
            if (startClassification is PointClassification.CW or PointClassification.CCW)
                return Array.Empty<VR>();


            var edgeStartBetween = IsBetween(segment.Start.X, edge.Start.X, segment.End.X) && IsBetween(segment.Start.Y, edge.Start.Y, segment.End.Y);
            var edgeEndBetween = IsBetween(segment.Start.X, edge.End.X, segment.End.X) && IsBetween(segment.Start.Y, edge.End.Y, segment.End.Y);
            var startBetween = IsBetween(edge.Start.X, segment.Start.X, edge.End.X) && IsBetween(edge.Start.Y, segment.Start.Y, edge.End.Y);
            var endBetween = IsBetween(edge.Start.X, segment.End.X, edge.End.X) && IsBetween(edge.Start.Y, segment.End.Y, edge.End.Y);

            var result = new List<VR>(4);

            if (edgeStartBetween)
                result.Add(edge.Start);
            if (edgeEndBetween)
                result.Add(edge.End);
            if (startBetween && !result.Contains(segment.Start))
                result.Add(segment.Start);
            if (endBetween && !result.Contains(segment.End))
                result.Add(segment.End);

            return result;
		}

        public static Rational GetSegmentPenalty(this RSeg segment, RPolygon polygon)
        {
            if (segment.Start.Equals(segment.End))
                return new Rational(int.MaxValue);

            var points = new Dictionary<VR, List<RSeg>>
            {
                {segment.Start, new List<RSeg>()},
                {segment.End, new List<RSeg>()},
            };
            foreach (var edge in polygon.Segments)
            {
                var vs = segment.GetIntersectionWithEdge(edge);
                foreach (var v in vs)
                {
                    var list = points.GetOrCreate(v, _ => new List<RSeg>());
                    list.Add(edge);
                }
            }

            var result = Rational.Zero;
            var pointsOrdered = points.OrderBy(x => x.Key.X).ThenBy(x => x.Key.Y).ToList();
            for (var i = 0; i < pointsOrdered.Count - 1; i++)
            {
                var (p, edges) = pointsOrdered[i];
                var (pNext, _) = pointsOrdered[i + 1];
                var v = pNext - p;
                if (edges.Count == 0)
                {
                    if (i == 0)
                    {
                        (p, edges) = pointsOrdered[i + 1];
                        (pNext, _) = pointsOrdered[i];
                        v = pNext - p;
                        if (edges.Count == 0)
                        {
                            if (pointsOrdered.Count == 2)
                            {
                                if (segment.Start.GetPositionToPolygon(polygon) == PointToPolygonPositionType.Outside)
                                    result += v.Length2;
                                continue;
                            }

                            throw new InvalidOperationException($"No edges for intermediate point {p}");
                        }
                    }
                    else
                        throw new InvalidOperationException($"No edges for intermediate point {p}");
                }


                if (edges.Count == 1)
                {
                    var classify = pNext.Classify(edges[0]);
                    switch (classify)
                    {
                        case PointClassification.CW:
                            break;
                        case PointClassification.CCW:
                            result += v.Length2;
                            break;
                        default:
                            if (i != 0)
                                throw new InvalidOperationException($"Bad classification for {pNext} to {edges[0]}");
                            switch (classify)
                            {
                                case PointClassification.BETWEEN:
                                case PointClassification.ORIGIN:
                                case PointClassification.DESTINATION:
                                    break;
                                default:
                                    throw new InvalidOperationException($"Bad classification for {pNext} to {edges[0]}");
                            }
                            break;
                    }
                }
                else if (edges.Count == 2)
                {
                    var isReversed = ReferenceEquals(edges[0], polygon.Segments[0]) && ReferenceEquals(edges[1], polygon.Segments[^1]);
                    var e1 = isReversed ? edges[1] : edges[0];
                    var e2 = isReversed ? edges[0] : edges[1];
                    var v1 = e1.ToVector();
                    var v2 = e2.ToVector();
                    var prodE1ToE2 = v1.VectorProdLength(v2);
                    var prodE1ToV = v1.VectorProdLength(v);
                    var prodE2ToV = v2.VectorProdLength(v);
                    bool isInside;
                    if (prodE1ToV.IsZero && v1.ScalarProd(v).IsNegative)
                        isInside = true;
                    else if (prodE1ToE2.IsPositive && !prodE1ToV.IsPositive)
                        isInside = false;
                    else if (prodE1ToE2.IsNegative && !prodE1ToV.IsNegative)
                        isInside = true;
                    else if (!prodE2ToV.IsNegative)
                        isInside = true;
                    else
                        isInside = false;

                    if (!isInside)
                        result += v.Length2;
                }
                else
                    throw new Exception($"Self-touch detected for intermediate point {p}. Edges count: {edges.Count}");
            }

            return result;
        }


        private static bool IsBetween(Rational a, Rational x, Rational b)
		{
            return !((a - x) * (b - x)).IsPositive;
		}

		public static double GetAngleMeasure(VR vec1, VR vec2)
		{
			var vectorAngleMeasure = 1 + vec1.ScalarProd(vec2)/Math.Sqrt(vec1.Length2*vec2.Length2);
			if (vec1.X*vec2.Y - vec1.Y*vec2.X < 0)
				vectorAngleMeasure = 4 - vectorAngleMeasure;
			return vectorAngleMeasure;
		}

		public static Rational GetSin(VR vec1, VR vec2)
		{
			if(vec1.Length2 != vec2.Length2)
				throw new Exception("vectors must be equal");
			return (vec1.X*vec2.Y - vec1.Y*vec2.X)/vec1.Length2;
		}

		public static Rational GetCos(VR vec1, VR vec2)
		{
			if (vec1.Length2 != vec2.Length2)
				throw new Exception("vectors must be equal");
			return (vec1.X * vec2.X + vec1.Y * vec2.Y)/vec1.Length2;
		}

		public static Rational? GetXIntersect(this RSeg segment, int y)
		{
			var A = segment.End - segment.Start;
			var B = segment.Start;

			if (A.Y == 0)
				return null;

			var intersection = A.X*(y - B.Y)/A.Y + B.X;
			if (IsBetween(segment.Start.X, intersection, segment.End.X))
				return intersection;
			return null;
		}

		public static Rational? GetYIntersect(this RSeg segment, int x)
		{
			var A = segment.End - segment.Start;
			var B = segment.Start;

			if (A.X == 0)
				return null;

			var intersection = A.Y * (x - B.X) / A.X + B.Y;

			if (IsBetween(segment.Start.Y, intersection, segment.End.Y))
				return intersection;
			return null;
		}
	}
}
