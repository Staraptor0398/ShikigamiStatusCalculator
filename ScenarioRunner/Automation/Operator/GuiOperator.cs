using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA2;
using ScenarioRunner.Execution;
using System;
using System.IO;

namespace ScenarioRunner.Automation.Operator
{
	public class GuiOperator
	{
		public void Open(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.GuiSession != null)
			{
				throw new InvalidOperationException("Gui.exe is already running.");
			}

			string guiExecutablePath = context.GuiExecutablePath;

			if (!File.Exists(guiExecutablePath))
			{
				throw new FileNotFoundException("Gui.exe was not found.", guiExecutablePath);
			}

			Application application = Application.Launch(guiExecutablePath);
			UIA2Automation automation = new UIA2Automation();

			context.GuiSession = new GuiSession(application, automation);
		}

		public void Close(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.GuiSession == null)
			{
				throw new InvalidOperationException("Gui.exe is not running.");
			}

			context.GuiSession.Application.Close();

			context.GuiSession.Dispose();
			context.GuiSession = null;
		}

		public Window GetMainWindow(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			var desktop = session.Automation.GetDesktop();
			var mainWindowElement = desktop.FindFirstDescendant(cf => cf.ByProcessId(session.Application.ProcessId).And(cf.ByControlType(ControlType.Window)).And(cf.ByAutomationId("MainForm")));

			if (mainWindowElement == null)
			{
				throw new InvalidOperationException("Gui.exe main window was not found.");
			}

			return mainWindowElement.AsWindow();
		}
	}
}
