namespace lib.Origami
{
	public enum PointToPolygonPositionType
	{
		Boundary,
		Inside,
		Outside
	}

    public enum PointClassification
    {
        CW,
        CCW,
        AFTER,
        BEFORE,
        BETWEEN,
        ORIGIN,
        DESTINATION
    };

	public static class PointToPolygonPositionExtensions
	{
		public static PointToPolygonPositionType GetPositionToPolygon(this VR p, RPolygon polygon)
		{
			var parity = true;
			for (var i = 0; i < polygon.Vertices.Length; i++)
			{
				var v1 = polygon.Vertices[i];
				var v2 = polygon.Vertices[(i + 1)%polygon.Vertices.Length];
				var segment = new RSeg(v1, v2);
				switch (ClassifyEdge(p, segment))
				{
					case EdgeType.TOUCHING:
						return PointToPolygonPositionType.Boundary;
					case EdgeType.CROSSING:
						parity = !parity;
						break;
				}
			}
			return parity ? PointToPolygonPositionType.Outside : PointToPolygonPositionType.Inside;
		}

		private enum EdgeType
		{
			CROSSING,
			INESSENTIAL,
			TOUCHING
		}

		private static EdgeType ClassifyEdge(VR a, RSeg e)
		{
			var v = e.Start;
			var w = e.End;
			switch (a.Classify(e))
			{
				case PointClassification.CW:
					return ((v.Y < a.Y) && (a.Y <= w.Y)) ? EdgeType.CROSSING : EdgeType.INESSENTIAL;
				case PointClassification.CCW:
					return ((w.Y < a.Y) && (a.Y <= v.Y)) ? EdgeType.CROSSING : EdgeType.INESSENTIAL;
				case PointClassification.BETWEEN:
				case PointClassification.ORIGIN:
				case PointClassification.DESTINATION:
					return EdgeType.TOUCHING;
				default:
					return EdgeType.INESSENTIAL;
			}
		}

		public static PointClassification Classify(this VR p, RSeg s)
		{
			var a = s.End - s.Start;
			var b = p - s.Start;
			var sa = a.X*b.Y - b.X*a.Y;
			if (sa.IsPositive)
				return PointClassification.CW;
			if (sa.IsNegative)
				return PointClassification.CCW;
			if ((a.X * b.X).IsNegative || (a.Y*b.Y).IsNegative)
				return PointClassification.BEFORE;
			if (a.Length2 < b.Length2)
				return PointClassification.AFTER;
			if (s.Start.Equals(p))
				return PointClassification.ORIGIN;
			if (s.End.Equals(p))
				return PointClassification.DESTINATION;
			return PointClassification.BETWEEN;
		}
    }
}
