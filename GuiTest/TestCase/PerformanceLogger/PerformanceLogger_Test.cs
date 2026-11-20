using Gui.Logging;
using System.Diagnostics;
using System.Text;

namespace GuiTest.TestCase
{
	[TestClass]
	public class PerformanceLogger_Test
	{
		private static readonly DateTime mTimestamp = new DateTime(2026, 11, 20, 12, 34, 56, 789);
		private static readonly Guid mSessionId = Guid.ParseExact("0123456789abcdef0123456789abcdef", "N");

		[TestMethod]
		public void FormatRejectsNullStatistics()
		{
			Assert.ThrowsExactly<ArgumentNullException>(() => PerformanceLogger.Format(mSessionId, null, mTimestamp));
		}

		[TestMethod]
		public void FormatEmptyStatistics()
		{
			string actual = PerformanceLogger.Format(mSessionId, new List<PerformanceStatistics>(), mTimestamp);

			string expected =
				"2026-11-20 12:34:56.789 Session=0123456789abcdef0123456789abcdef Event=Begin" + Environment.NewLine +
				"2026-11-20 12:34:56.789 Session=0123456789abcdef0123456789abcdef Event=End" + Environment.NewLine;

			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FormatSingleStatistics()
		{
			var statistics = new List<PerformanceStatistics>
			{
				new PerformanceStatistics
				{
					Target = "Calculation",
					Count = 2,
					TotalTicks = Stopwatch.Frequency * 3,
					MaxTicks = Stopwatch.Frequency * 2
				}
			};

			string actual = PerformanceLogger.Format(mSessionId, statistics, mTimestamp);

			string expected =
				"2026-11-20 12:34:56.789 Session=0123456789abcdef0123456789abcdef Event=Begin" + Environment.NewLine +
				"2026-11-20 12:34:56.789 Session=0123456789abcdef0123456789abcdef Target=Calculation Count=2 TotalMs=3000.000 AverageMs=1500.000 MaxMs=2000.000" + Environment.NewLine +
				"2026-11-20 12:34:56.789 Session=0123456789abcdef0123456789abcdef Event=End" + Environment.NewLine;

			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FormatMultipleStatistics()
		{
			var statistics = new List<PerformanceStatistics>
			{
				new PerformanceStatistics
				{
					Target = "Calculation",
					Count = 1,
					TotalTicks = Stopwatch.Frequency,
					MaxTicks = Stopwatch.Frequency
				},
				new PerformanceStatistics
				{
					Target = "SaveData.Load",
					Count = 2,
					TotalTicks = Stopwatch.Frequency * 2,
					MaxTicks = Stopwatch.Frequency
				}
			};

			string actual = PerformanceLogger.Format(mSessionId, statistics, mTimestamp);

			string[] lines = actual.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			Assert.AreEqual(4, lines.Length);

			Assert.IsTrue(lines[0].Contains("Event=Begin"));
			Assert.IsTrue(lines[1].Contains("Target=Calculation Count=1"));
			Assert.IsTrue(lines[2].Contains("Target=SaveData.Load Count=2"));
			Assert.IsTrue(lines[3].Contains("Event=End"));

			foreach (string line in lines)
			{
				Assert.IsTrue(line.Contains("Session=0123456789abcdef0123456789abcdef"));
				Assert.IsTrue(line.StartsWith("2026-11-20 12:34:56.789"));
			}
		}

		[TestMethod]
		public void WriteAppendsToExistingFile()
		{
			string directoryPath = Path.Combine(Path.GetTempPath(), "PerformanceLoggerTest_" + Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(directoryPath);

			try
			{
				string filePath = Path.Combine(directoryPath, "Performance.log");

				File.WriteAllText(filePath, "Existing" + Environment.NewLine, Encoding.UTF8);

				var statistics = new List<PerformanceStatistics>
				{
					new PerformanceStatistics
					{
						Target = "Calculation",
						Count = 1,
						TotalTicks = Stopwatch.Frequency,
						MaxTicks = Stopwatch.Frequency
					}
				};

				PerformanceLogger.Write(mSessionId, statistics, mTimestamp, filePath);

				string actual = File.ReadAllText(filePath, Encoding.UTF8);

				string expected = "Existing" + Environment.NewLine + PerformanceLogger.Format(mSessionId, statistics, mTimestamp);

				Assert.AreEqual(expected, actual);
			}
			finally
			{
				Directory.Delete(directoryPath, true);
			}
		}

		[TestMethod]
		public void WriteCreatesNewFile()
		{
			string directoryPath = Path.Combine(Path.GetTempPath(), "PerformanceLoggerTest_" + Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(directoryPath);

			try
			{
				string filePath = Path.Combine(directoryPath, "Performance.log");

				var statistics = new List<PerformanceStatistics>();

				PerformanceLogger.Write(mSessionId, statistics, mTimestamp, filePath);

				Assert.IsTrue(File.Exists(filePath));

				string actual = File.ReadAllText(filePath, Encoding.UTF8);
				string expected = PerformanceLogger.Format(mSessionId, statistics, mTimestamp);

				Assert.AreEqual(expected, actual);
			}
			finally
			{
				Directory.Delete(directoryPath, true);
			}
		}

		[TestMethod]
		public void WriteRejectsNullStatistics()
		{
			string filePath = Path.Combine(Path.GetTempPath(), "PerformanceLoggerTest_" + Guid.NewGuid().ToString("N") + ".log");

			Assert.ThrowsExactly<ArgumentNullException>(() => PerformanceLogger.Write(mSessionId, null, mTimestamp, filePath));

			Assert.IsFalse(File.Exists(filePath));
		}
	}
}
