using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using System;

namespace ScenarioRunner.Automation
{
	public class GuiSession : IDisposable
	{
		public Application Application { get; }
		public UIA2Automation Automation { get; }
		public Window MainWindow { get; }

		public GuiSession(Application application, UIA2Automation automation, Window mainWindow)
		{
			Application = application;
			Automation = automation;
			MainWindow = mainWindow;
		}

		public void Dispose()
		{
			Automation?.Dispose();
		}
	}
}
