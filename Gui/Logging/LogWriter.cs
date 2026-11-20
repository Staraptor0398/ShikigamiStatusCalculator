using Gui.Definition;
using System;
using System.IO;
using System.Text;

namespace Gui.Logging
{
	internal sealed class LogWriter
	{
		private readonly Action<string> mAppend;
		private readonly object mSyncRoot = new object();

		public LogWriter() : this(appendFile)
		{
		}

		public LogWriter(Action<string> append)
		{
			mAppend = append ?? throw new ArgumentNullException(nameof(append));
		}

		public bool TryWrite(string logLine)
		{
			lock (mSyncRoot)
			{
				try
				{
					mAppend(logLine);
					return true;
				}
				catch (Exception)
				{
					// ログ保存失敗は呼び出し元へ伝播させない。
					return false;
				}
			}
		}

		private static void appendFile(string logLine)
		{
			Directory.CreateDirectory(AppPath.LogDirectoryPath);

			File.AppendAllText(
				AppPath.LogFilePath,
				logLine + Environment.NewLine,
				Encoding.UTF8);
		}
	}
}
