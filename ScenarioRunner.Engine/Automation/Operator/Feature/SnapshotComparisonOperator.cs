using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class SnapshotComparisonOperator
	{
		private const int WAIT_TIMEOUT_MS = 5000;
		private const int WAIT_INTERVAL_MS = 100;

		private readonly GuiOperator mGuiOperator;
		private readonly ButtonOperator mButtonOperator;
		private readonly FileDialogOperator mFileDialogOperator;
		private readonly WindowWaiter mWindowWaiter;
		private readonly DataGridViewOperator mDataGridViewOperator;

		private Window mResultForm;

		private IReadOnlyDictionary<string, string[]> mComparisonResultRows;

		public SnapshotComparisonOperator()
		{
			mGuiOperator = new GuiOperator();
			mButtonOperator = new ButtonOperator();
			mFileDialogOperator = new FileDialogOperator();
			mWindowWaiter = new WindowWaiter();
			mDataGridViewOperator = new DataGridViewOperator();
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

			mResultForm = null;
			mComparisonResultRows = null;

			GuiSession session = context.GuiSession;
			int processId = session.Application.ProcessId;

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			AutomationElement compareSnapshotButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.COMPARE_SNAPSHOT, ControlType.Button);

			mButtonOperator.Click(compareSnapshotButton);

			Window fileSelectDialog = mWindowWaiter.WaitForWindow(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.AutomationId == AutomationIds.SnapshotCompareFileSelectDialog.ID);

			var elementMap = new AutomationElementMap(fileSelectDialog);

			AutomationElement browseBaseSnapshotButton = elementMap.Get(AutomationIds.SnapshotCompareFileSelectDialog.BROWSE_BASE_SNAPSHOT, ControlType.Button);

			AutomationElement browseTargetSnapshotButton = elementMap.Get(AutomationIds.SnapshotCompareFileSelectDialog.BROWSE_TARGET_SNAPSHOT, ControlType.Button);

			AutomationElement compareButton = elementMap.Get(AutomationIds.SnapshotCompareFileSelectDialog.COMPARE, ControlType.Button);

			mButtonOperator.Click(browseBaseSnapshotButton);

			Window baseFileDialog = mWindowWaiter.WaitForFileDialog(session);

			mFileDialogOperator.SelectLoadFile(baseFileDialog, context.ResolvePath(baseSnapshotPath));

			mButtonOperator.Click(browseTargetSnapshotButton);

			Window targetFileDialog = mWindowWaiter.WaitForFileDialog(session);

			mFileDialogOperator.SelectLoadFile(targetFileDialog, context.ResolvePath(targetSnapshotPath));

			waitForEnabled(compareButton);

			mButtonOperator.Click(compareButton);

			mResultForm = mWindowWaiter.WaitForWindow(session, element => element.Properties.ProcessId.ValueOrDefault == processId && element.AutomationId == AutomationIds.StatusComparisonResultForm.ID);
		}

		public void CompareSaved(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (string.IsNullOrWhiteSpace(context.SavedSnapshotBaseFilePath))
			{
				throw new InvalidOperationException("Saved BASE snapshot is not defined.");
			}

			if (string.IsNullOrWhiteSpace(context.SavedSnapshotTargetFilePath))
			{
				throw new InvalidOperationException("Saved TARGET snapshot is not defined.");
			}

			if (!File.Exists(context.SavedSnapshotBaseFilePath))
			{
				throw new FileNotFoundException("Saved BASE snapshot was not found.", context.SavedSnapshotBaseFilePath);
			}

			if (!File.Exists(context.SavedSnapshotTargetFilePath))
			{
				throw new FileNotFoundException("Saved TARGET snapshot was not found.", context.SavedSnapshotTargetFilePath);
			}

			Compare(context, context.SavedSnapshotBaseFilePath, context.SavedSnapshotTargetFilePath);
		}

		public void Check(GuiSession session, string statusName, string expectedDifference)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (mResultForm == null || !mResultForm.IsAvailable)
			{
				throw new InvalidOperationException("Snapshot comparison result window is not available.");
			}

			if (mComparisonResultRows == null)
			{
				mComparisonResultRows = mDataGridViewOperator.GetRowValues(mResultForm, AutomationIds.StatusComparisonResultForm.COMPARISON_RESULT);
			}

			string actualDifference = mDataGridViewOperator.GetCellValue(mComparisonResultRows, statusName, 1);

			if (actualDifference != expectedDifference)
			{
				throw new InvalidOperationException($"Snapshot comparison mismatch. " + $"Status: {statusName}, " + $"Expected: {expectedDifference}, " + $"Actual: {actualDifference}");
			}
		}

		private void waitForEnabled(AutomationElement element)
		{
			int elapsed = 0;

			while (elapsed < WAIT_TIMEOUT_MS)
			{
				if (element.Properties.IsEnabled.ValueOrDefault)
				{
					return;
				}

				Thread.Sleep(WAIT_INTERVAL_MS);
				elapsed += WAIT_INTERVAL_MS;
			}

			throw new InvalidOperationException($"Element was not enabled within {WAIT_TIMEOUT_MS} ms.");
		}
	}
}
