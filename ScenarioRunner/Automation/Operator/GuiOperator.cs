using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using ScenarioRunner.Execution;
using System;
using System.IO;

namespace ScenarioRunner.Automation.Operator
{
	public class GuiOperator
	{
		private const string MAIN_WINDOW_AUTOMATION_ID = "MainForm";

		private readonly WindowOperator mWindowOperator;

		public GuiOperator()
		{
			mWindowOperator = new WindowOperator();
		}

		public void Launch(ScenarioExecutonContext context)
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

		public void Open(ScenarioExecutonContext context)
		{
			Launch(context);

			Window mainWindow = GetMainWindow(context.GuiSession);

			mWindowOperator.SetBounds(mainWindow, context.GuiBounds);
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

			int processId = session.Application.ProcessId;

			return mWindowOperator.WaitForWindow(session, element => element.Properties.ProcessId.Value == processId && element.Properties.AutomationId.ValueOrDefault == MAIN_WINDOW_AUTOMATION_ID);
		}
	}
}
