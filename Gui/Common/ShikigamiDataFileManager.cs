using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gui.Common
{
	public class ShikigamiDataFileManager
	{
		private static readonly Encoding UTF8_WITHOUT_BOM = new UTF8Encoding(false);

		private const string RESOURCE_NAME = "Gui.Data.ShikigamiData.csv";

		private const int MAX_BROKEN_COUNT = 30;
		private const int MAX_BACKUP_COUNT = 10;

		public static void RestoreDefaultIfMissing()
		{
			if (File.Exists(AppPath.ShikigamiDataCsvPath))
			{
				return;
			}

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME))
			{
				if (stream == null)
				{
					Logger.Error($"Operation=式神データ復元 Message=埋め込みリソースが見つかりません。 Resource={RESOURCE_NAME}");
					return;
				}

				using (var reader = new StreamReader(stream, UTF8_WITHOUT_BOM))
				{
					File.WriteAllText(
						AppPath.ShikigamiDataCsvPath,
						reader.ReadToEnd(),
						UTF8_WITHOUT_BOM);
				}
			}

			Logger.Info("Operation=式神データ復元 Message=式神データCSVを埋め込みリソースから復元しました。");
		}

		public static void MoveBrokenFile()
		{
			if (!File.Exists(AppPath.ShikigamiDataCsvPath))
			{
				return;
			}

			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string brokenFileName = $"ShikigamiData_broken_{timestamp}.csv";
			string brokenFilePath = Path.Combine(AppPath.DataBrokenDirectoryPath, brokenFileName);

			File.Move(AppPath.ShikigamiDataCsvPath, brokenFilePath);

			Logger.Info($"Operation=式神データ退避 Message=破損した式神データCSVを退避しました。 Path={brokenFilePath}");
		}

		public static void CreateBackup()
		{
			if (!File.Exists(AppPath.ShikigamiDataCsvPath))
			{
				return;
			}

			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string backupFileName = $"ShikigamiData_{timestamp}.csv";
			string backupFilePath = Path.Combine(AppPath.DataBackupDirectoryPath, backupFileName);

			File.Copy(AppPath.ShikigamiDataCsvPath, backupFilePath, true);

			deleteOldBackups();
		}

		private static void deleteOldBrokens()
		{
			var brokenFiles = Directory.GetFiles(AppPath.DataBrokenDirectoryPath, "ShikigamiData_broken_*.csv").OrderByDescending(File.GetCreationTime).ToList();

			foreach (var file in brokenFiles.Skip(MAX_BROKEN_COUNT))
			{
				File.Delete(file);
			}
		}

		private static void deleteOldBackups()
		{
			var backupFiles = Directory.GetFiles(AppPath.DataBackupDirectoryPath, "ShikigamiData_*.csv").OrderByDescending(File.GetCreationTime).ToList();

			foreach (var file in backupFiles.Skip(MAX_BACKUP_COUNT))
			{
				File.Delete(file);
			}
		}
	}
}
