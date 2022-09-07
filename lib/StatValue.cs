using System;
using System.Collections.Generic;
using System.Globalization;

namespace lib
{
    public class StatValue
    {
        public StatValue()
        {
        }

        public StatValue(IEnumerable<double> values)
        {
            foreach (var value in values) Add(value);
        }

        public StatValue(IEnumerable<int> values)
        {
            foreach (var value in values) Add(value);
        }

        public StatValue(long count, double sum, double sum2, double min, double max)
        {
            Count = count;
            Sum = sum;
            Sum2 = sum2;
            Min = min;
            Max = max;
        }

        public long Count { get; private set; }
        public double Sum { get; private set; }
        public double Sum2 { get; private set; }
        public double Min { get; private set; } = double.PositiveInfinity;
        public double Max { get; private set; } = double.NegativeInfinity;

        /// <summary>
        ///     Standard deviation = sigma = sqrt(Dispersion)
        ///     sigma^2 = /(n-1)
        /// </summary>
        public double StdDeviation => Math.Sqrt(Dispersion);

        /// <summary>
        ///     D = sum{(xi - mean)^2}/(count-1) =
        ///     = sum{xi^2 - 2 xi mean + mean^2} / (count-1) =
        ///     = (sum2 + sum*sum/count - 2 sum * sum / count) / (count-1) =
        ///     = (sum2 - sum*sum / count) / (count - 1)
        /// </summary>
        public double Dispersion => (Sum2 - Sum * Sum / Count) / (Count - 1);

        /// <summary>
        ///     2 sigma confidence interval for mean value of random value
        /// </summary>
        public double ConfIntervalSize2Sigma => 2 * StdDeviation / Math.Sqrt(Count);

        public double Mean => Sum / Count;

        public void Add(double value)
        {
            Count++;
            Sum += value;
            Sum2 += value * value;
            Min = Math.Min(Min, value);
            Max = Math.Max(Max, value);
        }

        public void AddAll(StatValue value)
        {
            Count += value.Count;
            Sum += value.Sum;
            Sum2 += value.Sum2;
            Min = Math.Min(Min, value.Min);
            Max = Math.Max(Max, value.Max);
        }

        public string FormatCompact(double x)
        {
            if (Math.Abs(x) > 100) return x.ToString("0");
            if (Math.Abs(x) > 10) return x.ToString("0.#");
            if (Math.Abs(x) > 1) return x.ToString("0.##");
            if (Math.Abs(x) > 0.1) return x.ToString("0.###");
            if (Math.Abs(x) > 0.01) return x.ToString("0.####");
            return x.ToString(CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            if (Count == 0) return "NA";
            return $"{FormatCompact(Mean)} sigma={FormatCompact(StdDeviation)}";
        }

        public string ToDetailedString(bool humanReadable = true)
        {
            if (Count == 0) return "NA";
            if (humanReadable)
                return $"{Mean.ToString(CultureInfo.InvariantCulture)} " +
                       $"disp={StdDeviation.ToString(CultureInfo.InvariantCulture)} " +
                       $"min..max={Min.ToString(CultureInfo.InvariantCulture)}..{Max.ToString(CultureInfo.InvariantCulture)} " +
                       $"confInt={ConfIntervalSize2Sigma.ToString(CultureInfo.InvariantCulture)} " +
                       $"count={Count}";
            FormattableString line = $"{Mean}\t{StdDeviation}\t{ConfIntervalSize2Sigma}\t{Count}";
            return line.ToString(CultureInfo.InvariantCulture);
        }

        public StatValue Clone()
        {
            return new StatValue(Count, Sum, Sum2, Min, Max);
        }
    }
}