using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class SnapshotComparisonOperator
	{
		private readonly GuiOperator mGuiOperator;
		private readonly ButtonOperator mButtonOperator;
		private readonly FileDialogOperator mFileDialogOperator;
		private readonly WindowWaiter mWindowWaiter;

		public SnapshotComparisonOperator()
		{
			mGuiOperator = new GuiOperator();
			mButtonOperator = new ButtonOperator();
			mFileDialogOperator = new FileDialogOperator();
			mWindowWaiter = new WindowWaiter();
		}

		public void Compare(ScenarioExecutonContext context, string baseSnapshotPath, string targetSnapshotPath)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (string.IsNullOrWhiteSpace(baseSnapshotPath))
			{
				throw new ArgumentException("Base snapshot path is empty.", nameof(baseSnapshotPath));
			}

			if (string.IsNullOrWhiteSpace(targetSnapshotPath))
			{
				throw new ArgumentException("Target snapshot path is empty.", nameof(targetSnapshotPath));
			}

			Window mainWindow = mGuiOperator.GetMainWindow(context.GuiSession);
			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.COMPARE_SNAPSHOT);

			int processId = context.GuiSession.Application.ProcessId;
			Window fileSelectDialog = mWindowWaiter.WaitForWindow(context.GuiSession, element => element.Properties.ProcessId.ValueOrDefault == processId && element.AutomationId == AutomationIds.SnapshotCompareFileSelectDialog.ID);

			mButtonOperator.Click(fileSelectDialog, AutomationIds.SnapshotCompareFileSelectDialog.BROWSE_BASE_SNAPSHOT);

			Window baseFileDialog = mWindowWaiter.WaitForFileDialog(context.GuiSession);
			mFileDialogOperator.SelectFile(baseFileDialog, context.ResolvePath(baseSnapshotPath));

			mButtonOperator.Click(fileSelectDialog, AutomationIds.SnapshotCompareFileSelectDialog.BROWSE_TARGET_SNAPSHOT);

			Window targetFileDialog = mWindowWaiter.WaitForFileDialog(context.GuiSession);
			mFileDialogOperator.SelectFile(targetFileDialog, context.ResolvePath(targetSnapshotPath));

			mButtonOperator.Click(fileSelectDialog, AutomationIds.SnapshotCompareFileSelectDialog.COMPARE);

			mWindowWaiter.WaitForWindow(context.GuiSession, element => element.Properties.ProcessId.ValueOrDefault == processId && element.AutomationId == AutomationIds.StatusComparisonResultForm.ID);
		}
	}
}
