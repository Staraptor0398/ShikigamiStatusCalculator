using System;
using System.Globalization;

namespace Gui.Logging
{
	public static class Logger
	{
		private static readonly Guid mSessionId = Guid.NewGuid();
		private static readonly LogWriter mWriter = new LogWriter();

		public static Guid SessionId => mSessionId;

		public static void Info(string operation, string eventName, string details = null)
		{
			write("INFO", operation, eventName, details);
		}

		public static void Warning(string operation, string eventName, string details = null)
		{
			write("WARNING", operation, eventName, details);
		}

		public static void Error(string operation, string eventName, string details = null)
		{
			write("ERROR", operation, eventName, details);
		}

		public static void Error(string operation, string eventName, Exception exception, string details = null)
		{
			string exceptionText;

			try
			{
				exceptionText = exception?.ToString() ?? "null";
			}
			catch (Exception)
			{
				exceptionText = exception?.GetType().FullName ?? "null";
			}

			string exceptionDetails = $"Exception={exceptionText}";

			if (!string.IsNullOrEmpty(details))
			{
				exceptionDetails = $"{details} {exceptionDetails}";
			}

			write("ERROR", operation, eventName, exceptionDetails);
		}

		internal static string Format(string level, string operation, string eventName, string details, DateTime timestamp, Guid sessionId)
		{
			string logLine =
				$"{timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} " +
				$"[{level}] SessionId={sessionId:N} Operation={sanitize(operation)} Event={sanitize(eventName)}";

			if (!string.IsNullOrEmpty(details))
			{
				logLine += $" {sanitize(details)}";
			}

			return logLine;
		}

		private static string sanitize(string value)
		{
			return (value ?? "")
				.Replace("\\", "\\\\")
				.Replace("\r", "\\r")
				.Replace("\n", "\\n");
		}

		private static void write(string level, string operation, string eventName, string details)
		{
			try
			{
				string logLine = Format(level, operation, eventName, details, DateTime.Now, mSessionId);

				mWriter.TryWrite(logLine);
			}
			catch (Exception)
			{
				// 診断ログの出力失敗で本来の処理を妨げない。
			}
		}
	}
}
