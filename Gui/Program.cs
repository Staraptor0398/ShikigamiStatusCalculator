using Gui.Form;
using Gui.Logging;
using Gui.ShikigamiData;
using System;
using System.Windows.Forms;

namespace Gui
{
	internal static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			ApplicationDirectoryInitializer.Initialize();
			ShikigamiDataFileManager.RestoreDefaultIfMissing();

			Application.Run(new MainForm());

			try
			{
				Logger.Info("Operation=Shutdown Event=Application.Run.End");
			}
			catch
			{
				// 調査ログの出力失敗でアプリケーション終了を妨げない。
			}
		}
	}
}
