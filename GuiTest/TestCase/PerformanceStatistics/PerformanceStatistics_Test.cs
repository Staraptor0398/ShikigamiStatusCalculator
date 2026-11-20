using Gui.Logging;
using System.Diagnostics;

namespace GuiTest.TestCase
{
	[TestClass]
	public class PerformanceStatistics_Test
	{
		[TestMethod]
		public void TotalMillisecondsConvertsTicks()
		{
			var statistics = new PerformanceStatistics
			{
				Target = "Test",
				Count = 1,
				TotalTicks = Stopwatch.Frequency * 2,
				MaxTicks = Stopwatch.Frequency
			};

			Assert.AreEqual(2000.0, statistics.TotalMilliseconds, 0.000001);
		}

		[TestMethod]
		public void AverageMillisecondsCalculatesAverage()
		{
			var statistics = new PerformanceStatistics
			{
				Target = "Test",
				Count = 4,
				TotalTicks = Stopwatch.Frequency * 2,
				MaxTicks = Stopwatch.Frequency
			};

			Assert.AreEqual(500.0, statistics.AverageMilliseconds, 0.000001);
		}

		[TestMethod]
		public void MaxMillisecondsConvertsTicks()
		{
			var statistics = new PerformanceStatistics
			{
				Target = "Test",
				Count = 2,
				TotalTicks = Stopwatch.Frequency * 3,
				MaxTicks = Stopwatch.Frequency * 2
			};

			Assert.AreEqual(2000.0, statistics.MaxMilliseconds, 0.000001);
		}

		[TestMethod]
		public void AverageMillisecondsReturnsZeroWhenCountIsZero()
		{
			var statistics = new PerformanceStatistics
			{
				Target = "Test",
				Count = 0,
				TotalTicks = 0,
				MaxTicks = 0
			};

			Assert.AreEqual(0.0, statistics.AverageMilliseconds);
		}

		[TestMethod]
		public void MillisecondsReturnZeroWhenTicksAreZero()
		{
			var statistics = new PerformanceStatistics
			{
				Target = "Test",
				Count = 1,
				TotalTicks = 0,
				MaxTicks = 0
			};

			Assert.AreEqual(0.0, statistics.TotalMilliseconds);
			Assert.AreEqual(0.0, statistics.AverageMilliseconds);
			Assert.AreEqual(0.0, statistics.MaxMilliseconds);
		}
	}
}
