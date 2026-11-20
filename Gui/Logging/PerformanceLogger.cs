using Gui.Definition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gui.Logging
{
	public static class PerformanceLogger
	{
		public static void Write(Guid sessionId, IReadOnlyList<PerformanceStatistics> statistics)
		{
			if (statistics == null)
			{
				throw new ArgumentNullException(nameof(statistics));
			}

			Write(sessionId, statistics, DateTime.Now, AppPath.PerformanceLogFilePath);
		}

		internal static void Write(Guid sessionId, IReadOnlyList<PerformanceStatistics> statistics, DateTime timestamp, string filePath)
		{
			string content = Format(sessionId, statistics, timestamp);

			File.AppendAllText(filePath, content, Encoding.UTF8);
		}

		internal static string Format(Guid sessionId, IReadOnlyList<PerformanceStatistics> statistics, DateTime timestamp)
		{
			if (statistics == null)
			{
				throw new ArgumentNullException(nameof(statistics));
			}

			var builder = new StringBuilder();

			string timestampText = timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

			builder.AppendLine($"{timestampText} Session={sessionId:N} Event=Begin");

			foreach (PerformanceStatistics item in statistics)
			{
				builder.AppendLine(
					$"{timestampText} Session={sessionId:N} " +
					$"Target={item.Target} " +
					$"Count={item.Count} " +
					$"TotalMs={item.TotalMilliseconds:F3} " +
					$"AverageMs={item.AverageMilliseconds:F3} " +
					$"MaxMs={item.MaxMilliseconds:F3}");
			}

			builder.AppendLine($"{timestampText} Session={sessionId:N} Event=End");

			return builder.ToString();
		}
	}
}
