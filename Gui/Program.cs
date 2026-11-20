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
			Logger.Info("Application", "Started");

			MainForm mainForm;

			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				ApplicationDirectoryInitializer.Initialize();
				ShikigamiDataFileManager.RestoreDefaultIfMissing();

				mainForm = new MainForm();
			}
			catch (Exception ex)
			{
				Logger.Error("Application", "InitializationFailed", ex);
				throw;
			}

			Application.Run(mainForm);

			Logger.Info("Application", "Closed");
		}
	}
}
