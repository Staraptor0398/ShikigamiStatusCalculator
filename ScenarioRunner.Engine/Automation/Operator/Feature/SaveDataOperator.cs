using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ScenarioRunner.Automation.Operator.Feature
{
	public class SaveDataOperator
	{
		private const string MITAMA_SAVE_TYPE = "御魂セット保存データ";
		private const string BUILD_SAVE_TYPE = "ビルド保存データ";
		private const string SNAPSHOT_SAVE_TYPE = "計算結果スナップショット";

		private const string MITAMA_EXTENSION = ".mitama.json";
		private const string BUILD_EXTENSION = ".build.json";
		private const string SNAPSHOT_EXTENSION = ".snapshot.json";

		private const int WAIT_TIMEOUT_MS = 5000;
		private const int WAIT_INTERVAL_MS = 100;

		private readonly ButtonOperator mButtonOperator;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly TextBoxOperator mTextBoxOperator;
		private readonly FileDialogOperator mFileDialogOperator;
		private readonly GuiOperator mGuiOperator;

		private readonly WindowWaiter mWindowWaiter;

		public SaveDataOperator()
		{
			mButtonOperator = new ButtonOperator();
			mComboBoxOperator = new ComboBoxOperator();
			mTextBoxOperator = new TextBoxOperator();
			mFileDialogOperator = new FileDialogOperator();
			mGuiOperator = new GuiOperator();

			mWindowWaiter = new WindowWaiter();
		}

		public void LoadMitama(ScenarioExecutonContext context, string filePath)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			load(context, MITAMA_SAVE_TYPE, context.ResolvePath(filePath));
		}

		public void LoadSavedMitama(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			validateSavedFilePath(context.SavedMitamaFilePath, "Mitama");

			load(context, MITAMA_SAVE_TYPE, context.SavedMitamaFilePath);
		}

		public void LoadSavedBuild(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			validateSavedFilePath(context.SavedBuildFilePath, "Build");

			load(context, BUILD_SAVE_TYPE, context.SavedBuildFilePath);
		}

		public void SaveMitama(ScenarioExecutonContext context)
		{
			string filePath = save(context, MITAMA_SAVE_TYPE, "Mitama", MITAMA_EXTENSION);

			context.SavedMitamaFilePath = filePath;
		}

		public void SaveBuild(ScenarioExecutonContext context)
		{
			string filePath = save(context, BUILD_SAVE_TYPE, "Build", BUILD_EXTENSION);

			context.SavedBuildFilePath = filePath;
		}

		public void SaveSnapshot(ScenarioExecutonContext context, string target)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			switch (target)
			{
				case "BASE":
					context.SavedSnapshotBaseFilePath = save(context, SNAPSHOT_SAVE_TYPE, "Snapshot_BASE", SNAPSHOT_EXTENSION);
					break;
				case "TARGET":
					context.SavedSnapshotTargetFilePath = save(context, SNAPSHOT_SAVE_TYPE, "Snapshot_TARGET", SNAPSHOT_EXTENSION);
					break;
				default:
					throw new InvalidOperationException($"Unknown snapshot target: {target}");
			}
		}

		public void CheckSaveDataLevel(ScenarioExecutonContext context, string expectedLevel)
		{
			validateContext(context);

			GuiSession session = context.GuiSession;

			AutomationElement saveButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SAVE, ControlType.Button);

			bool saveEnabled = saveButton.Properties.IsEnabled.ValueOrDefault;

			if (expectedLevel == "NONE")
			{
				if (saveEnabled)
				{
					throw new InvalidOperationException("SaveData level mismatch. Expected=NONE, but save operation is enabled.");
				}

				return;
			}

			if (!saveEnabled)
			{
				throw new InvalidOperationException($"SaveData level mismatch. Expected={expectedLevel}, but save operation is disabled.");
			}

			IReadOnlyList<string> expectedSaveTypes = getExpectedSaveTypes(expectedLevel);

			mButtonOperator.Click(saveButton);

			Window saveDialog = getSaveDialog(session);

			try
			{
				IReadOnlyList<string> actualSaveTypes = mComboBoxOperator.GetItems(saveDialog, AutomationIds.SaveDataSaveDialog.SAVE_TYPE);

				if (!actualSaveTypes.SequenceEqual(expectedSaveTypes))
				{
					throw new InvalidOperationException(
						$"SaveData level mismatch. " +
						$"Expected={expectedLevel}, " +
						$"ExpectedSaveTypes=[{string.Join(", ", expectedSaveTypes)}], " +
						$"ActualSaveTypes=[{string.Join(", ", actualSaveTypes)}]");
				}
			}
			finally
			{
				mButtonOperator.Click(saveDialog, AutomationIds.SaveDataSaveDialog.CANCEL);

				mWindowWaiter.WaitForWindowClosed(saveDialog);
			}
		}

		private void load(ScenarioExecutonContext context, string loadType, string filePath)
		{
			validateContext(context);
			validateLoadFilePath(filePath);

			GuiSession session = context.GuiSession;

			string operation = $"LOAD [{loadType}]";

			Stopwatch totalStopwatch = Stopwatch.StartNew();
			Stopwatch sectionStopwatch = Stopwatch.StartNew();

			AutomationElement mainLoadButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.LOAD, ControlType.Button);

			mButtonOperator.Click(mainLoadButton);

			Window loadDialog = getLoadDialog(session);

			logFileDialogPerf(operation, "Main LOAD click -> Load dialog detected", sectionStopwatch, totalStopwatch);

			var elementMap = new AutomationElementMap(loadDialog);

			AutomationElement loadTypeElement = elementMap.Get(AutomationIds.SaveDataLoadDialog.LOAD_TYPE, ControlType.ComboBox);
			AutomationElement browseButton = elementMap.Get(AutomationIds.SaveDataLoadDialog.BROWSE, ControlType.Button);
			AutomationElement filePathElement = elementMap.Get(AutomationIds.SaveDataLoadDialog.FILE_PATH);
			AutomationElement loadButton = elementMap.Get(AutomationIds.SaveDataLoadDialog.LOAD, ControlType.Button);

			selectLoadType(loadTypeElement, loadType);

			logFileDialogPerf(operation, "Load dialog preparation", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(browseButton);

			FileDialogElements fileDialogElements = mWindowWaiter.WaitForFileDialog(session);

			logFileDialogPerf(operation, "Browse click -> FileDialog detected", sectionStopwatch, totalStopwatch);

			mFileDialogOperator.SelectLoadFile(fileDialogElements, filePath);

			logFileDialogPerf(operation, "FileDialog detected -> SelectLoadFile completed", sectionStopwatch, totalStopwatch);

			waitForFilePath(filePathElement, filePath);

			logFileDialogPerf(operation, "SelectLoadFile completed -> File path reflected", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(loadButton);

			mWindowWaiter.WaitForWindowClosed(loadDialog);

			logFileDialogPerf(operation, "LOAD click -> Load dialog closed", sectionStopwatch, totalStopwatch);

			totalStopwatch.Stop();

			Console.WriteLine($"[FileDialogPerf] {operation} | TOTAL | {totalStopwatch.Elapsed.TotalMilliseconds:F1} ms");
		}

		private string save(ScenarioExecutonContext context, string saveType, string fileNameSuffix, string extension)
		{
			validateContext(context);

			string filePath = createUniqueSaveFilePath(context, fileNameSuffix, extension);

			GuiSession session = context.GuiSession;

			string operation = $"SAVE [{saveType}]";

			Stopwatch totalStopwatch = Stopwatch.StartNew();
			Stopwatch sectionStopwatch = Stopwatch.StartNew();

			AutomationElement mainSaveButton = mGuiOperator.GetMainElement(session, AutomationIds.MainForm.SAVE, ControlType.Button);

			mButtonOperator.Click(mainSaveButton);

			Window saveDialog = getSaveDialog(session);

			logFileDialogPerf(operation, "Main SAVE click -> Save dialog detected", sectionStopwatch, totalStopwatch);

			var elementMap = new AutomationElementMap(saveDialog);

			AutomationElement saveTypeElement = elementMap.Get(AutomationIds.SaveDataSaveDialog.SAVE_TYPE, ControlType.ComboBox);
			AutomationElement browseButton = elementMap.Get(AutomationIds.SaveDataSaveDialog.BROWSE, ControlType.Button);
			AutomationElement filePathElement = elementMap.Get(AutomationIds.SaveDataSaveDialog.FILE_PATH);
			AutomationElement saveButton = elementMap.Get(AutomationIds.SaveDataSaveDialog.SAVE, ControlType.Button);

			selectSaveType(saveTypeElement, saveType);

			logFileDialogPerf(operation, "Save dialog preparation", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(browseButton);

			FileDialogElements fileDialogElements = mWindowWaiter.WaitForFileDialog(session);

			logFileDialogPerf(operation, "Browse click -> FileDialog detected", sectionStopwatch, totalStopwatch);

			mFileDialogOperator.SelectSaveFile(fileDialogElements, filePath);

			logFileDialogPerf(operation, "FileDialog detected -> SelectSaveFile completed", sectionStopwatch, totalStopwatch);

			waitForFilePath(filePathElement, filePath);

			logFileDialogPerf(operation, "SelectSaveFile completed -> File path reflected", sectionStopwatch, totalStopwatch);

			mButtonOperator.Click(saveButton);

			mWindowWaiter.WaitForWindowClosed(saveDialog);

			logFileDialogPerf(operation, "SAVE click -> Save dialog closed", sectionStopwatch, totalStopwatch);

			waitForFileCreated(filePath);

			logFileDialogPerf(operation, "Save dialog closed -> File created", sectionStopwatch, totalStopwatch);

			totalStopwatch.Stop();

			Console.WriteLine($"[FileDialogPerf] {operation} | TOTAL | {totalStopwatch.Elapsed.TotalMilliseconds:F1} ms");

			return filePath;
		}

		private static void logFileDialogPerf(string operation, string phase, Stopwatch sectionStopwatch, Stopwatch totalStopwatch)
		{
			Console.WriteLine(
				$"[FileDialogPerf] {operation} | {phase} | " +
				$"Section={sectionStopwatch.Elapsed.TotalMilliseconds:F1} ms | " +
				$"Total={totalStopwatch.Elapsed.TotalMilliseconds:F1} ms");

			sectionStopwatch.Restart();
		}

		private void selectLoadType(AutomationElement loadTypeElement, string loadType)
		{
			mComboBoxOperator.SelectItem(loadTypeElement, loadType);

			string selectedLoadType = mComboBoxOperator.GetValue(loadTypeElement);

			if (!string.Equals(selectedLoadType, loadType, StringComparison.Ordinal))
			{
				throw new InvalidOperationException($"SaveData load type was not selected correctly. " + $"Expected={loadType}, Actual={selectedLoadType}");
			}
		}

		private void selectSaveType(AutomationElement saveTypeElement, string saveType)
		{
			mComboBoxOperator.SelectItem(saveTypeElement, saveType);

			string selectedSaveType = mComboBoxOperator.GetValue(saveTypeElement);

			if (!string.Equals(selectedSaveType, saveType, StringComparison.Ordinal))
			{
				throw new InvalidOperationException($"SaveData save type was not selected correctly. " + $"Expected={saveType}, Actual={selectedSaveType}");
			}
		}

		private string createUniqueSaveFilePath(ScenarioExecutonContext context, string fileNameSuffix, string extension)
		{
			string scenarioName = Path.GetFileNameWithoutExtension(context.ScenarioPath);

			string directoryPath = Path.Combine(Path.GetTempPath(), "ShikigamiStatusCalculator", "ScenarioRunner", scenarioName);

			Directory.CreateDirectory(directoryPath);

			string fileName = $"{scenarioName}_{fileNameSuffix}_" + $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_" + $"{Guid.NewGuid():N}{extension}";

			return Path.Combine(directoryPath, fileName);
		}

		private void waitForFilePath(AutomationElement filePathElement, string expectedFilePath)
		{
			int elapsed = 0;
			string actualFilePath = "";

			while (elapsed < WAIT_TIMEOUT_MS)
			{
				actualFilePath = mTextBoxOperator.GetText(filePathElement);

				if (string.Equals(actualFilePath, expectedFilePath, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				Thread.Sleep(WAIT_INTERVAL_MS);
				elapsed += WAIT_INTERVAL_MS;
			}

			throw new InvalidOperationException(
				$"SaveData file path was not selected correctly within {WAIT_TIMEOUT_MS} ms. " +
				$"Expected={expectedFilePath}, Actual={actualFilePath}" +
				$"{Environment.NewLine}{mFileDialogOperator.LastSelectionInfo}");
		}

		private void waitForFileCreated(string filePath)
		{
			int elapsed = 0;

			while (elapsed < WAIT_TIMEOUT_MS)
			{
				if (File.Exists(filePath))
				{
					return;
				}

				Thread.Sleep(WAIT_INTERVAL_MS);
				elapsed += WAIT_INTERVAL_MS;
			}

			throw new InvalidOperationException($"Saved file was not created within {WAIT_TIMEOUT_MS} ms: {filePath}");
		}

		private Window getSaveDialog(GuiSession session)
		{
			return mWindowWaiter.WaitForProcessWindow(session, element => isSaveDialog(element));
		}

		private Window getLoadDialog(GuiSession session)
		{
			return mWindowWaiter.WaitForProcessWindow(session, element => isLoadDialog(element));
		}

		private bool isSaveDialog(AutomationElement element)
		{
			return element.Properties.AutomationId.ValueOrDefault == AutomationIds.SaveDataSaveDialog.ID;
		}

		private bool isLoadDialog(AutomationElement element)
		{
			return element.Properties.AutomationId.ValueOrDefault == AutomationIds.SaveDataLoadDialog.ID;
		}

		private void validateContext(ScenarioExecutonContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.GuiSession == null)
			{
				throw new InvalidOperationException("Gui.exe is not running.");
			}
		}

		private void validateLoadFilePath(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("SaveData file path is empty.", nameof(filePath));
			}

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("SaveData file was not found.", filePath);
			}
		}

		private void validateSavedFilePath(string filePath, string saveDataName)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new InvalidOperationException($"Saved {saveDataName} file is not defined.");
			}

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException($"Saved {saveDataName} file was not found.", filePath);
			}
		}

		private IReadOnlyList<string> getExpectedSaveTypes(string level)
		{
			switch (level)
			{
				case "MITAMA":
					return new[]
					{
						MITAMA_SAVE_TYPE
					};
				case "BUILD":
					return new[]
					{
						MITAMA_SAVE_TYPE,
						BUILD_SAVE_TYPE
					};
				case "SNAPSHOT":
					return new[]
					{
						MITAMA_SAVE_TYPE,
						BUILD_SAVE_TYPE,
						SNAPSHOT_SAVE_TYPE
					};
				default:
					throw new InvalidOperationException($"Unknown SaveData level: {level}");
			}
		}
	}
}
