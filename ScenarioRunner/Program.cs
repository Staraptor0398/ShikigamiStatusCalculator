using ScenarioRunner.Form;
using ScenarioRunner.Startup;
using System;
using System.Windows.Forms;

namespace ScenarioRunner
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

			string guiExecutablePath = GuiExecutablePathResolver.Resolve();

			Application.Run(new MainForm(guiExecutablePath));
		}
	}
}
