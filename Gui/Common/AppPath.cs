using System;
using System.IO;

namespace Gui.Common
{
	public static class AppPath
	{
		public const string ShikigamiDataFileName = "ShikigamiData.csv";
		public const string LogFileName = "ShikigamiApp.log";

		public static string BaseDirectory
		{
			get
			{
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}

		public static string ShikigamiDataCsvPath
		{
			get
			{
				return Path.Combine(
					BaseDirectory,
					ShikigamiDataFileName);
			}
		}

		public static string LogFilePath
		{
			get
			{
				return Path.Combine(
					BaseDirectory,
					LogFileName);
			}
		}
	}
}