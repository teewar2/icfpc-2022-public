using System;
using System.Diagnostics.Contracts;

namespace lib.Origami
{
    public class RSeg
    {
        public readonly VR Start, End;
	    public int Id;


        public VR Direction {  get { return End - Start; } }

        public RSeg(VR start, VR end)
        {
            Start = start;
            End = end;
        }

        public VR Middle => (Start + End) / 2;
		public RSeg Invert() => new RSeg(End, Start);
		public VR[] Ends()
		{
			return new[] { Start, End };
		}
		public bool IsEndpoint(VR p)
		{
			return p.Equals(Start) || p.Equals(End);
		}

		public Rational Distance2To(VR p)
		{
			return Arithmetic.Distance2(p, this);
		}

	    public VR ToVector()
	    {
		    return End -Start;
	    }

        public Rational Length2
        {
            get
            {
                var result = (End.X - Start.X) * (End.X - Start.X) +
                (End.Y - Start.Y) * (End.Y - Start.Y);

                result.Reduce();
                return result;
            }
        }

        public double IrrationalLength
        {
            get
            {
                return Math.Sqrt((double)Length2);
            }
        }

		public static implicit operator RSeg(string s)
		{
			return Parse(s);
		}

		public static RSeg Parse(string s)
		{
			var parts = s.Split(' ');
			if (parts.Length != 2) throw new FormatException(s);
			return new RSeg(VR.Parse(parts[0]), VR.Parse(parts[1]));
		}

		public RSeg Reflect(RSeg mirror)
		{
			return new RSeg(Start.Reflect(mirror), End.Reflect(mirror));
		}

		public override string ToString()
		{
			return $"{Start} {End}";
		}

		[Pure]
		public RSeg Move(Rational shiftX, Rational shiftY)
		{
			return new RSeg(Start.Move(shiftX, shiftY), End.Move(shiftX, shiftY));
		}
		[Pure]
		public RSeg Move(VR shift)
		{
			return Move(shift.X, shift.Y);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return Start.GetHashCode() ^ End.GetHashCode();
			}
		}

		public override bool Equals(object? obj)
		{
			var segment = obj as RSeg;
			if(segment == null)
				return false;
			return Start.Equals(segment.Start) && End.Equals(segment.End) || End.Equals(segment.Start) && Start.Equals(segment.End);
		}
	}
}
