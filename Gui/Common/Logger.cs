using System;
using System.IO;
using System.Text;

namespace Gui.Common
{
	public static class Logger
	{
		public static void Info(string message)
		{
			write("INFO", message);
		}

		public static void Warning(string message)
		{
			write("Warning", message);
		}

		public static void Error(string message)
		{
			write("Error", message);
		}

		private static void write(string level, string message)
		{
			string logLine = $"{DateTime.Now:yyyy--MM-dd HH:mm:ss.fff} [{level}] {message}";

			File.AppendAllText(
				AppPath.LogFilePath,
				logLine + Environment.NewLine,
				Encoding.UTF8);
		}
	}
}
