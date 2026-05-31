using System.IO;

namespace Gui.Common
{
	public static class ApplicationDirectoryInitializer
	{
		public static void Initialize()
		{
			Directory.CreateDirectory(AppPath.DataDirectoryPath);
			Directory.CreateDirectory(AppPath.DataBackupDirectoryPath);
			Directory.CreateDirectory(AppPath.DataBrokenDirectoryPath);

			Directory.CreateDirectory(AppPath.SaveDataDirectoryPath);
			Directory.CreateDirectory(AppPath.BuildSaveDataDirectoryPath);
			Directory.CreateDirectory(AppPath.MitamaSetSaveDataDirectoryPath);
			Directory.CreateDirectory(AppPath.SnapshotSaveDataDirectoryPath);

			Directory.CreateDirectory(AppPath.LogDirectoryPath);
		}
	}
}
