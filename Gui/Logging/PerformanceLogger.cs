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

			var builder = new StringBuilder();

			string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

			builder.AppendLine($"{timestamp} Session={sessionId:N} Event=Begin");

			foreach (PerformanceStatistics item in statistics)
			{
				builder.AppendLine(
					$"{timestamp} Session={sessionId:N} " +
					$"Target={item.Target} " +
					$"Count={item.Count} " +
					$"TotalMs={item.TotalMilliseconds:F3} " +
					$"AverageMs={item.AverageMilliseconds:F3} " +
					$"MaxMs={item.MaxMilliseconds:F3}");
			}

			builder.AppendLine($"{timestamp} Session={sessionId:N} Event=End");

			File.AppendAllText(AppPath.PerformanceLogFilePath, builder.ToString(), Encoding.UTF8);
		}
	}
}
