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

		public Window MainWindow { get; set; }

		public GuiSession(Application application, UIA2Automation automation)
		{
			Application = application;
			Automation = automation;
		}

		public void Dispose()
		{
			MainWindow = null;
			Automation?.Dispose();
		}
	}
}
