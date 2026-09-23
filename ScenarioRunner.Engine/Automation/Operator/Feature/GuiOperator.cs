using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA2;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;
using System.IO;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class GuiOperator
	{
		private readonly WindowOperator mWindowOperator;

		private readonly WindowWaiter mWindowWaiter;

		public GuiOperator()
		{
			mWindowOperator = new WindowOperator();

			mWindowWaiter = new WindowWaiter();
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

			if (session.MainWindow != null)
			{
				return session.MainWindow;
			}

			int processId = session.Application.ProcessId;

			Window mainWindow = mWindowWaiter.WaitForWindow(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.Properties.AutomationId.ValueOrDefault == AutomationIds.MainForm.ID);

			session.MainWindow = mainWindow;

			return mainWindow;
		}

		public AutomationElementMap GetMainElementMap(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (session.MainElementMap == null)
			{
				Window mainWindow = GetMainWindow(session);

				session.MainElementMap = new AutomationElementMap(mainWindow);
			}

			return session.MainElementMap;
		}

		public AutomationElement GetMainElement(GuiSession session, string automationId)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (string.IsNullOrWhiteSpace(automationId))
			{
				throw new ArgumentException(
					"AutomationId is empty.",
					nameof(automationId));
			}

			if (session.MainElementMap != null)
			{
				return session.MainElementMap.Get(automationId);
			}

			Window mainWindow = GetMainWindow(session);

			AutomationElement element = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId));

			if (element == null)
			{
				throw new InvalidOperationException($"Automation element was not found: {automationId}");
			}

			return element;
		}

		public AutomationElement GetMainElement(GuiSession session, string automationId, ControlType controlType)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (string.IsNullOrWhiteSpace(automationId))
			{
				throw new ArgumentException("AutomationId is empty.", nameof(automationId));
			}

			if (session.MainElementMap != null)
			{
				return session.MainElementMap.Get(automationId, controlType);
			}

			Window mainWindow = GetMainWindow(session);

			AutomationElement element = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId).And(cf.ByControlType(controlType)));

			if (element == null)
			{
				throw new InvalidOperationException($"Automation element was not found: {automationId}, " + $"ControlType={controlType}");
			}

			return element;
		}
	}
}
