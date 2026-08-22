using FlaUI.Core;
using FlaUI.UIA2;
using ScenarioRunner.Execution;
using System;
using System.IO;

namespace ScenarioRunner.Automation.Operator
{
	public class GuiOperator
	{
		private readonly string mGuiExecutablePath;

		public GuiOperator(string guiExecutablePath)
		{
			mGuiExecutablePath = guiExecutablePath;
		}

		public void Open(ScenarioExecutonContext context)
		{
			if (context.GuiSession != null)
			{
				throw new InvalidOperationException("Gui.exe is already running.");
			}

			if (!File.Exists(mGuiExecutablePath))
			{
				throw new FileNotFoundException("Gui.exe was not found.", mGuiExecutablePath);
			}

			Application application = Application.Launch(mGuiExecutablePath);
			var automation = new UIA2Automation();

			try
			{
				var mainWindow = application.GetMainWindow(automation);

				if (mainWindow == null)
				{
					throw new InvalidOperationException("MainForm could not be found.");
				}

				context.GuiSession = new GuiSession(application, automation, mainWindow);
			}
			catch
			{
				automation.Dispose();
				application.Close();
				throw;
			}
		}

		public void Close(ScenarioExecutonContext context)
		{
			if (context.GuiSession == null)
			{
				throw new InvalidOperationException("Gui.exe is not running.");
			}

			context.GuiSession.Application.Close();
			context.GuiSession.Dispose();
			context.GuiSession = null;
		}
	}
}
