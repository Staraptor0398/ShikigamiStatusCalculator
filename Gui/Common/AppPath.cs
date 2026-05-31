using System;
using System.IO;

namespace Gui.Common
{
	public static class AppPath
	{
		public const string DataDirectoryName = "Data";
		public const string DataBackupDirectoryName = "Backup";
		public const string DataBrockenDirectoryName = "Broken";

		public const string ShikigamiDataFileName = "ShikigamiData.csv";

		public static string BaseDirectory
		{
			get
			{
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}

		public static string DataDirectoryPath
		{
			get
			{
				return Path.Combine(
					BaseDirectory,
					DataDirectoryName);
			}
		}

		public static string DataBackupDirectoryPath
		{
			get
			{
				return Path.Combine(
					DataDirectoryPath,
					DataBackupDirectoryName);
			}
		}

		public static string DataBrokenDirectoryPath
		{
			get
			{
				return Path.Combine(
					DataDirectoryPath,
					DataBrockenDirectoryName);
			}
		}

		public static string ShikigamiDataCsvPath
		{
			get
			{
				return Path.Combine(
					DataDirectoryPath,
					ShikigamiDataFileName);
			}
		}

		public static string LogDirectoryPath
		{
			get
			{
				return Path.Combine(
					BaseDirectory,
					LogFileDefinition.LogDirectoryName);
			}
		}

		public static string LogFilePath
		{
			get
			{
				string fileName =
					LogFileDefinition.LogFilePrefix +
					DateTime.Now.ToString("yyyyMMdd") +
					LogFileDefinition.LogFileExtension;

				return Path.Combine(
					LogDirectoryPath,
					fileName);
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
