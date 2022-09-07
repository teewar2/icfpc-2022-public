using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace lib.Origami
{
	public class RPolygon : IEquatable<RPolygon>
	{
		public int Id;
		public readonly VR[] Vertices;
		public readonly RSeg[] Segments;

		public bool IsReflected = false;

		public RPolygon(params VR[] vertices)
		{
			Vertices = vertices;
			Segments = BuildSegments(vertices).ToArray();
		}

		public bool Equals(RPolygon? other)
		{
            if (ReferenceEquals(null, other)) return false;

			if (other.Vertices.Length != Vertices.Length)
				return false;
			return !Vertices.Where((t, i) => !other.Vertices[i].Equals(t)).Any();
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((RPolygon) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 0;
				foreach (var vector in Vertices)
					hashCode = hashCode * 397 ^ vector.GetHashCode();
				return hashCode;
			}
		}

		private static List<RSeg> BuildSegments(VR[] vertices)
		{
			var segments = new List<RSeg>();
			for (int i = 0; i < vertices.Length; i++)
			{
				var vertex1 = vertices[i];
				var vertex2 = vertices[(i + 1)%vertices.Length];
				segments.Add(new RSeg(vertex1, vertex2));
			}
			return segments;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(Vertices.Length.ToString());
			sb.Append(Vertices.StrJoin(Environment.NewLine));
			return sb.ToString();
		}

		/*
		public static Polygon Parse(StringReader reader)
		{
			var vCount = int.Parse(reader.ReadLine() ?? "0");
			var ps = Enumerable.Range(0, vCount)
				.Select(i => reader.ReadLine())
				.Select(Vector.Parse)
				.ToArray();
			return new Polygon(ps);
		}
		*/

		public RPolygon Move(Rational shiftX, Rational shiftY)
		{
			return new RPolygon(Vertices.Select(p => new VR(p.X + shiftX, p.Y + shiftY)).ToArray());
		}

		public RPolygon Reflect(RSeg mirror)
		{
			var polygon = new RPolygon(Vertices.Select(v => v.Reflect(mirror)).ToArray()) { IsReflected = !IsReflected, Id = Id };
			for (int i = 0; i < Segments.Length; i++)
			{
				polygon.Segments[i].Id = Segments[i].Id;
			}
			return polygon;
		}

		public Rational GetUnsignedSquare()
		{
			var s = GetSignedSquare();
			return s > 0 ? s : -s;
		}

		public Rational GetSignedSquare() => GetSignedSquare(Vertices);

		public static Rational GetSignedSquare(IList<VR> vertices)
		{
			Rational sum = 0;
			for (int i = 0; i < vertices.Count; i++)
			{
				var p1 = vertices[i];
				var p2 = vertices[(i + 1) % vertices.Count];
				sum += (p1.X - p2.X) * (p1.Y + p2.Y) / 2;
			}
			return sum;
		}

		public bool IsConvex()
		{
			var signedSq = GetSignedSquare();
			for (int i = 0; i < Segments.Length; i++)
			{
				var thisEdge = Segments[i];
				var nextEdge = Segments[(i + 1)%Segments.Length];
				var prod = thisEdge.ToVector().VectorProdLength(nextEdge.ToVector());
				if ((signedSq > 0 && prod <= 0) || (signedSq < 0 && prod >= 0))
					return false;
			}
			return true;
		}

		public RPolygon RemoveExtraVertices()
		{
			var vertices = new List<VR>(Vertices);
			while (true)
			{
				var changed = false;
				for (int i = 1; i < vertices.Count + 1; i++)
				{
					var thisVertex = vertices[i % vertices.Count];
					var thisEdge = new RSeg(vertices[(i - 1) % vertices.Count], thisVertex);
					var nextEdge = new RSeg(thisVertex, vertices[(i + 1) % vertices.Count]);
					var prod = thisEdge.ToVector().VectorProdLength(nextEdge.ToVector());
					if (prod == 0)
					{
						vertices.Remove(thisVertex);
						changed = true;
						break;
					}
				}
				if (!changed)
					break;
			}
			return new RPolygon(vertices.ToArray());
		}

		public RPolygon GetConvexBoundary()
		{
			var vertices = Vertices.ToList();
			var zero = vertices.OrderBy(v => v.Y).ThenBy(v => v.X).First();
			vertices = vertices.OrderByDescending(v => (v - zero).ScalarProd(new VR(1, 0))/(v - zero).Length).ToList();
			var signedSq = GetSignedSquare(vertices);
			while (true)
			{
				var changed = false;
				for (int i = 1; i < vertices.Count + 1; i++)
				{
					var thisVertex = vertices[i%vertices.Count];
					var thisEdge = new RSeg(vertices[(i - 1)% vertices.Count], thisVertex);
					var nextEdge = new RSeg(thisVertex, vertices[(i + 1)% vertices.Count]);
					var prod = thisEdge.ToVector().VectorProdLength(nextEdge.ToVector());
					if ((signedSq > 0 && prod <= 0) || (signedSq < 0 && prod >= 0))
					{
						vertices.Remove(thisVertex);
						changed = true;
						break;
					}
				}
				if (!changed)
					break;
			}
			return new RPolygon(vertices.ToArray());
		}
	}
}
