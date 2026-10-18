using System.Diagnostics;

namespace Gui.Logging
{
	public sealed class PerformanceStatistics
	{
		public string Target { get; set; }

		public long Count { get; set; }

		public long TotalTicks { get; set; }

		public long MaxTicks { get; set; }

		public double TotalMilliseconds
		{
			get
			{
				return TotalTicks * 1000.0 / Stopwatch.Frequency;
			}
		}

		public double AverageMilliseconds
		{
			get
			{
				return Count == 0 ? 0 : TotalMilliseconds / Count;
			}
		}

		public double MaxMilliseconds
		{
			get
			{
				return MaxTicks * 1000.0 / Stopwatch.Frequency;
			}
		}
	}
}
