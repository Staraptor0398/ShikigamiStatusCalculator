using System;
using System.IO;

namespace Gui.Common
{
	public static class AppPath
	{
		public const string ShikigamiDataFileName = "ShikigamiData.csv";

		public static string ShikigamiDataCsvPath
		{
			get
			{
				return Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					ShikigamiDataFileName);
			}
		}
	}
}