using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

			const string OPERATION = "COMPARE SNAPSHOT";

			Stopwatch totalStopwatch = Stopwatch.StartNew();
			Stopwatch sectionStopwatch = Stopwatch.StartNew();

			AutomationElement compareSnapshotButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.COMPARE_SNAPSHOT, ControlType.Button);

			logFileDialogPerf(OPERATION, "Main COMPARE button lookup", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(compareSnapshotButton);

			logFileDialogPerf(OPERATION, "Main COMPARE click", sectionStopwatch, totalStopwatch);

			Window fileSelectDialog = mWindowWaiter.WaitForProcessWindow(session, element => element.AutomationId == AutomationIds.SnapshotCompareFileSelectDialog.ID);

			logFileDialogPerf(OPERATION, "Wait for File select dialog", sectionStopwatch, totalStopwatch);

			var elementMap = new AutomationElementMap(fileSelectDialog);

			AutomationElement browseBaseSnapshotButton = elementMap.Get(AutomationIds.SnapshotCompareFileSelectDialog.BROWSE_BASE_SNAPSHOT, ControlType.Button);
			AutomationElement browseTargetSnapshotButton = elementMap.Get(AutomationIds.SnapshotCompareFileSelectDialog.BROWSE_TARGET_SNAPSHOT, ControlType.Button);
			AutomationElement compareButton = elementMap.Get(AutomationIds.SnapshotCompareFileSelectDialog.COMPARE, ControlType.Button);

			logFileDialogPerf(OPERATION, "File select dialog preparation", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(browseBaseSnapshotButton);

			logFileDialogPerf(OPERATION, "BASE Browse click", sectionStopwatch, totalStopwatch);

			FileDialogElements baseFileDialogElements = mWindowWaiter.WaitForFileDialog(session);

			logFileDialogPerf(OPERATION, "Wait for BASE FileDialog", sectionStopwatch, totalStopwatch);

			mFileDialogOperator.SelectLoadFile(baseFileDialogElements, context.ResolvePath(baseSnapshotPath));

			logFileDialogPerf(OPERATION, "BASE SelectLoadFile", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(browseTargetSnapshotButton);

			logFileDialogPerf(OPERATION, "TARGET Browse click", sectionStopwatch, totalStopwatch);

			FileDialogElements targetFileDialogElements = mWindowWaiter.WaitForFileDialog(session);

			logFileDialogPerf(OPERATION, "Wait for TARGET FileDialog", sectionStopwatch, totalStopwatch);

			mFileDialogOperator.SelectLoadFile(targetFileDialogElements, context.ResolvePath(targetSnapshotPath));

			logFileDialogPerf(OPERATION, "TARGET SelectLoadFile", sectionStopwatch, totalStopwatch);

			waitForEnabled(compareButton);

			logFileDialogPerf(OPERATION, "Wait for Compare button enabled", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(compareButton);

			logFileDialogPerf(OPERATION, "COMPARE click", sectionStopwatch, totalStopwatch);

			mResultForm = mWindowWaiter.WaitForProcessWindow(session, element => element.AutomationId == AutomationIds.StatusComparisonResultForm.ID);

			logFileDialogPerf(OPERATION, "Wait for Result form", sectionStopwatch, totalStopwatch);

			totalStopwatch.Stop();

			Console.WriteLine($"[FileDialogPerf] {OPERATION} | TOTAL | {totalStopwatch.Elapsed.TotalMilliseconds:F1} ms");
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
				throw new InvalidOperationException($"Snapshot comparison mismatch. " + $"Status: {statusName}, " + $"Expected={expectedDifference}, " + $"Actual: {actualDifference}");
			}
		}

		private static void logFileDialogPerf(string operation, string phase, Stopwatch sectionStopwatch, Stopwatch totalStopwatch)
		{
			Console.WriteLine(
				$"[FileDialogPerf] {operation} | {phase} | " +
				$"Section={sectionStopwatch.Elapsed.TotalMilliseconds:F1} ms | " +
				$"Total={totalStopwatch.Elapsed.TotalMilliseconds:F1} ms");

			sectionStopwatch.Restart();
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
