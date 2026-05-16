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

		public static string SaveDataDirectoryPath
		{
			get
			{
				return Path.Combine(
					BaseDirectory,
					SaveDataFileDefinition.SaveDataDirectoryName);
			}
		}

		public static string BuildSaveDataDirectoryPath
		{
			get
			{
				return Path.Combine(
					SaveDataDirectoryPath,
					SaveDataFileDefinition.BuildDirectoryName);
			}
		}

		public static string MitamaSetSaveDataDirectoryPath
		{
			get
			{
				return Path.Combine(
					SaveDataDirectoryPath,
					SaveDataFileDefinition.MitamaSetDirectoryName);
			}
		}

		public static string SnapshotSaveDataDirectoryPath
		{
			get
			{
				return Path.Combine(
					SaveDataDirectoryPath,
					SaveDataFileDefinition.SnapshotDirectoryName);
			}
		}
	}
}
