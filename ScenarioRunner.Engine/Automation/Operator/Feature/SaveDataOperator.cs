using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Definition;
using ScenarioRunner.Automation.Waiter;
using ScenarioRunner.Execution;
using System;
using System.IO;
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

		private void load(ScenarioExecutonContext context, string loadType, string filePath)
		{
			validateContext(context);
			validateLoadFilePath(filePath);

			GuiSession session = context.GuiSession;
			int processId = session.Application.ProcessId;

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.LOAD);

			Window loadDialog = getLoadDialog(session, processId);

			selectLoadType(loadDialog, loadType);

			mButtonOperator.Click(loadDialog, AutomationIds.SaveDataLoadDialog.BROWSE);

			Window fileDialog = mWindowWaiter.WaitForFileDialog(session);
			mFileDialogOperator.SelectFile(fileDialog, filePath);

			waitForFilePath(loadDialog, AutomationIds.SaveDataLoadDialog.FILE_PATH, filePath);

			mButtonOperator.Click(loadDialog, AutomationIds.SaveDataLoadDialog.LOAD);

			mWindowWaiter.WaitForWindowClosed(session, element => isLoadDialog(element, processId));
		}

		private string save(ScenarioExecutonContext context, string saveType, string fileNameSuffix, string extension)
		{
			validateContext(context);

			string filePath = createUniqueSaveFilePath(context, fileNameSuffix, extension);

			GuiSession session = context.GuiSession;
			int processId = session.Application.ProcessId;

			Window mainWindow = mGuiOperator.GetMainWindow(session);

			mButtonOperator.Click(mainWindow, AutomationIds.MainForm.SAVE);

			Window saveDialog = getSaveDialog(session, processId);

			selectSaveType(saveDialog, saveType);

			mButtonOperator.Click(saveDialog, AutomationIds.SaveDataSaveDialog.BROWSE);

			Window fileDialog = mWindowWaiter.WaitForFileDialog(session);
			mFileDialogOperator.SelectSaveFile(fileDialog, filePath);

			waitForFilePath(saveDialog, AutomationIds.SaveDataSaveDialog.FILE_PATH, filePath);

			mButtonOperator.Click(saveDialog, AutomationIds.SaveDataSaveDialog.SAVE);

			mWindowWaiter.WaitForWindowClosed(session, element => isSaveDialog(element, processId));

			waitForFileCreated(filePath);

			return filePath;
		}

		private void selectLoadType(Window loadDialog, string loadType)
		{
			mComboBoxOperator.SelectItem(loadDialog, AutomationIds.SaveDataLoadDialog.LOAD_TYPE, loadType);

			string selectedLoadType = mComboBoxOperator.GetValue(loadDialog, AutomationIds.SaveDataLoadDialog.LOAD_TYPE);

			if (!string.Equals(selectedLoadType, loadType, StringComparison.Ordinal))
			{
				throw new InvalidOperationException($"SaveData load type was not selected correctly. Expected={loadType}, Actual={selectedLoadType}");
			}
		}

		private void selectSaveType(Window saveDialog, string saveType)
		{
			mComboBoxOperator.SelectItem(saveDialog, AutomationIds.SaveDataSaveDialog.SAVE_TYPE, saveType);

			string selectedSaveType = mComboBoxOperator.GetValue(saveDialog, AutomationIds.SaveDataSaveDialog.SAVE_TYPE);

			if (!string.Equals(selectedSaveType, saveType, StringComparison.Ordinal))
			{
				throw new InvalidOperationException($"SaveData save type was not selected correctly. Expected={saveType}, Actual={selectedSaveType}");
			}
		}

		private string createUniqueSaveFilePath(ScenarioExecutonContext context, string fileNameSuffix, string extension)
		{
			string scenarioName = Path.GetFileNameWithoutExtension(context.ScenarioPath);

			string directoryPath = Path.Combine(
				Path.GetTempPath(),
				"ShikigamiStatusCalculator",
				"ScenarioRunner",
				scenarioName);

			Directory.CreateDirectory(directoryPath);

			string fileName = $"{scenarioName}_{fileNameSuffix}_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}{extension}";

			return Path.Combine(directoryPath, fileName);
		}

		private void waitForFilePath(Window dialog, string automationId, string expectedFilePath)
		{
			int elapsed = 0;
			string actualFilePath = "";

			while (elapsed < WAIT_TIMEOUT_MS)
			{
				actualFilePath = mTextBoxOperator.GetText(dialog, automationId);

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

		private Window getSaveDialog(GuiSession session, int processId)
		{
			return mWindowWaiter.WaitForWindow(session, element => isSaveDialog(element, processId));
		}

		private Window getLoadDialog(GuiSession session, int processId)
		{
			return mWindowWaiter.WaitForWindow(session, element => isLoadDialog(element, processId));
		}

		private bool isSaveDialog(AutomationElement element, int processId)
		{
			return
				element.Properties.ProcessId.ValueOrDefault == processId &&
				element.Properties.AutomationId.ValueOrDefault == AutomationIds.SaveDataSaveDialog.ID;
		}

		private bool isLoadDialog(AutomationElement element, int processId)
		{
			return
				element.Properties.ProcessId.ValueOrDefault == processId &&
				element.Properties.AutomationId.ValueOrDefault == AutomationIds.SaveDataLoadDialog.ID;
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
	}
}
