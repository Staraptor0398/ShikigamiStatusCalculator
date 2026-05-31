using Gui.Common;
using Gui.Form;
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
		}
	}
}
