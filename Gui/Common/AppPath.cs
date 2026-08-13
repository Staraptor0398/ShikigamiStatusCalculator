using System;
using System.IO;

namespace Gui.Common
{
	public static class AppPath
	{
		public const string DATA_DIRECTORY_NAME = "Data";
		public const string DATA_BACKUP_DIRECTORY_NAME = "Backup";
		public const string DATA_BROKEN_DIRECTORY_NAME = "Broken";

		public const string SHIKIGAMI_DATA_FILE_NAME = "ShikigamiData.csv";

		public const string APP_VERSION_FILE_NAME = "AppVersion.txt";

#if DEBUG
		public const string TEST_SOURCE_PATH = "W:\\TestSource";
		public const string CALCULATION_TEST_SOURCE_DIRECTORY_PATH = TEST_SOURCE_PATH + "\\Calculation";
#endif

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
					DATA_DIRECTORY_NAME);
			}
		}

		public static string AppVersionFilePath
		{
			get
			{
				return Path.Combine(
					DataDirectoryPath,
					APP_VERSION_FILE_NAME);
			}
		}

		public static string DataBackupDirectoryPath
		{
			get
			{
				return Path.Combine(
					DataDirectoryPath,
					DATA_BACKUP_DIRECTORY_NAME);
			}
		}

		public static string DataBrokenDirectoryPath
		{
			get
			{
				return Path.Combine(
					DataDirectoryPath,
					DATA_BROKEN_DIRECTORY_NAME);
			}
		}

		public static string ShikigamiDataCsvPath
		{
			get
			{
				return Path.Combine(
					DataDirectoryPath,
					SHIKIGAMI_DATA_FILE_NAME);
			}
		}

		public static string LogDirectoryPath
		{
			get
			{
				return Path.Combine(
					BaseDirectory,
					LogFileDefinition.LOG_DIRECTORY_NAME);
			}
		}

		public static string LogFilePath
		{
			get
			{
				string fileName =
					LogFileDefinition.LOG_FILE_PREFIX +
					DateTime.Now.ToString("yyyyMMdd") +
					LogFileDefinition.LOG_FILE_EXTENSION;

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
					SaveDataFileDefinition.SAVE_DATA_DIRECTORY_NAME);
			}
		}

		public static string BuildSaveDataDirectoryPath
		{
			get
			{
				return Path.Combine(
					SaveDataDirectoryPath,
					SaveDataFileDefinition.BUILD_DIRECTORY_NAME);
			}
		}

		public static string MitamaSetSaveDataDirectoryPath
		{
			get
			{
				return Path.Combine(
					SaveDataDirectoryPath,
					SaveDataFileDefinition.MITAMA_SET_DIRECTORY_NAME);
			}
		}

		public static string SnapshotSaveDataDirectoryPath
		{
			get
			{
				return Path.Combine(
					SaveDataDirectoryPath,
					SaveDataFileDefinition.SNAPSHOT_DIRECTORY_NAME);
			}
		}
	}
}
