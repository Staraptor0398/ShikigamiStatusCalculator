using Gui.ControlSet;
using Gui.Converter;
using Gui.Definition;
using Gui.Dialog;
using Gui.Form.Control;
using Gui.Function;
using Gui.Logging;
using Gui.Model;
using Gui.Operator;
using Gui.ShikigamiData;
using Gui.Validation;
using SaveData.Access;
using SaveData.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

#if DEBUG
using SaveData.Model.Development;
#endif

namespace Gui.Form
{
	public partial class MainForm : System.Windows.Forms.Form
	{
		/****************************************************************************************************
		  UI入力コントロール生成
		****************************************************************************************************/
		private MitamaSlotInputControl[] createMitamaSlotInputControls()
		{
			return new MitamaSlotInputControl[]
			{
				createMitamaSlotInputControl(
					cmbMainStat1, txtMainVal1,
					createSubStatInputControl(cmbSubStat11, txtSubVal11),
					createSubStatInputControl(cmbSubStat21, txtSubVal21),
					createSubStatInputControl(cmbSubStat31, txtSubVal31),
					createSubStatInputControl(cmbSubStat41, txtSubVal41)),

				createMitamaSlotInputControl(
					cmbMainStat2, txtMainVal2,
					createSubStatInputControl(cmbSubStat12, txtSubVal12),
					createSubStatInputControl(cmbSubStat22, txtSubVal22),
					createSubStatInputControl(cmbSubStat32, txtSubVal32),
					createSubStatInputControl(cmbSubStat42, txtSubVal42)),

				createMitamaSlotInputControl(
					cmbMainStat3, txtMainVal3,
					createSubStatInputControl(cmbSubStat13, txtSubVal13),
					createSubStatInputControl(cmbSubStat23, txtSubVal23),
					createSubStatInputControl(cmbSubStat33, txtSubVal33),
					createSubStatInputControl(cmbSubStat43, txtSubVal43)),

				createMitamaSlotInputControl(
					cmbMainStat4, txtMainVal4,
					createSubStatInputControl(cmbSubStat14, txtSubVal14),
					createSubStatInputControl(cmbSubStat24, txtSubVal24),
					createSubStatInputControl(cmbSubStat34, txtSubVal34),
					createSubStatInputControl(cmbSubStat44, txtSubVal44)),

				createMitamaSlotInputControl(
					cmbMainStat5, txtMainVal5,
					createSubStatInputControl(cmbSubStat15, txtSubVal15),
					createSubStatInputControl(cmbSubStat25, txtSubVal25),
					createSubStatInputControl(cmbSubStat35, txtSubVal35),
					createSubStatInputControl(cmbSubStat45, txtSubVal45)),

				createMitamaSlotInputControl(
					cmbMainStat6, txtMainVal6,
					createSubStatInputControl(cmbSubStat16, txtSubVal16),
					createSubStatInputControl(cmbSubStat26, txtSubVal26),
					createSubStatInputControl(cmbSubStat36, txtSubVal36),
					createSubStatInputControl(cmbSubStat46, txtSubVal46))
			};
		}

		private static MitamaSlotInputControl createMitamaSlotInputControl(ComboBox mainStatComboBox, TextBox mainValueTextBox, SubStatInputControl subStat1, SubStatInputControl subStat2, SubStatInputControl subStat3, SubStatInputControl subStat4)
		{
			return new MitamaSlotInputControl
			{
				MainStatComboBox = mainStatComboBox,
				MainValueTextBox = mainValueTextBox,
				SubStats = new SubStatInputControl[]
				{
					subStat1,
					subStat2,
					subStat3,
					subStat4
				}
			};
		}

		private static SubStatInputControl createSubStatInputControl(ComboBox typeComboBox, TextBox valueTextBox)
		{
			return new SubStatInputControl
			{
				TypeComboBox = typeComboBox,
				ValueTextBox = valueTextBox
			};
		}

		private ComboBox[] createSetEffectComboBoxes()
		{
			return new ComboBox[]
			{
				cmbSetBonus1,
				cmbSetBonus2,
				cmbSetBonus3
			};
		}

		private ComboBox[] createUniqueEffectComboBoxes()
		{
			return new ComboBox[]
			{
				cmbUnique1,
				cmbUnique2,
				cmbUnique3,
				cmbUnique4,
				cmbUnique5,
				cmbUnique6
			};
		}

		/****************************************************************************************************
		  フィールド・プロパティ
		****************************************************************************************************/
		private readonly PerformanceTracker mPerformanceTracker = new PerformanceTracker();

		private ComboBoxOperator mComboBoxOperator = null;

		private MitamaInputFunction mMitamaInputFunction = null;

		private ShikigamiSelectionOperator mShikigamiSelectionOperator = null;

		private ShikigamiSelectionFunction mShikigamiSelectionFunction = null;

		private CalculationResultFunction mCalculationResultFunction = null;

		private CalculationFunction mCalculationFunction = null;

		private SaveDataButtonFunction mSaveDataButtonFunction = null;

		private SaveDataSaveFunction mSaveDataSaveFunction = null;

		private SaveDataLoadFunction mSaveDataLoadFunction = null;

		private SaveDataApplyFunction mSaveDataApplyFunction = null;

		private readonly Guid mPerformanceSessionId = Guid.NewGuid();

		private MainFormOperationState mOperationState = MainFormOperationState.IDLE;

		private MitamaSlotInputControl[] mMitamaSlotInputControls = null;

		private ComboBox[] mSetEffectComboBoxes = null;

		private ComboBox[] mUniqueEffectComboBoxes = null;

#if DEBUG
		private CalculationTestSource mLastCalculationTestSource = null;
#endif
		/****************************************************************************************************
		  コンストラクタ
		****************************************************************************************************/
		public MainForm()
		{
			using (mPerformanceTracker.Track("MainForm.Constructor"))
			{
				using (mPerformanceTracker.Track("MainForm.InitializeComponent"))
				{
					InitializeComponent();
				}

				btnSave.EnabledChanged += btnSave_EnabledChanged;
				btnLoad.EnabledChanged += btnLoad_EnabledChanged;

#if DEBUG
				initializeDevelopmentControls();
#endif

				string version;

				using (mPerformanceTracker.Track("MainForm.LoadAppVersion"))
				{
					version = loadAppVersion();
				}

				using (mPerformanceTracker.Track("MainForm.SetWindowTitle"))
				{
					this.Text = $"{this.Text} {version}";
				}
			}
		}

		/****************************************************************************************************
		  バージョン表示
		****************************************************************************************************/
		private string loadAppVersion()
		{
			if (!File.Exists(AppPath.AppVersionFilePath))
			{
				return "";
			}

			return File.ReadAllText(AppPath.AppVersionFilePath).Trim();
		}

		/****************************************************************************************************
		  MainForm状態遷移
		****************************************************************************************************/
		private void setOperationState(MainFormOperationState state)
		{
			if (mOperationState == state)
			{
				return;
			}

			MainFormOperationState previousState = mOperationState;
			mOperationState = state;

			Logger.Info($"Operation=MainForm状態遷移 Previous={previousState} Current={state}");

			updateSaveButtonEnabled("OperationState");
		}

		/****************************************************************************************************
		  式神ステータス表示
		****************************************************************************************************/
		private void cmbShikigami_SelectedIndexChanged(object sender, EventArgs e)
		{
			mShikigamiSelectionFunction?.HandleShikigamiSelectedIndexChanged();
		}

		/****************************************************************************************************
		  ステータス計算
		****************************************************************************************************/
		private void btnCalc_Click(object sender, EventArgs e)
		{
			using (mPerformanceTracker.Track("Calculation.Execute"))
			{
				CalculationResultDto calculationResult;

#if DEBUG
				CalculationTestSource calculationTestSource;
#endif

				try
				{
					bool calculationSucceeded;

#if DEBUG
					calculationSucceeded = mCalculationFunction.TryExecute(out calculationResult, out calculationTestSource);
#else
					calculationSucceeded = mCalculationFunction.TryExecute(out calculationResult);
#endif

					if (!calculationSucceeded)
					{
						return;
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Operation=ステータス計算 Message={ex}");

					MessageBox.Show(
						"ステータス計算中に予期しないエラーが発生しました。",
						"ステータス計算",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);

					return;
				}

				mCalculationResultFunction.SetResult(calculationResult);

#if DEBUG
				mLastCalculationTestSource = calculationTestSource;
#endif

				using (mPerformanceTracker.Track("Calculation.UpdateSaveButton"))
				{
					updateSaveButtonEnabled("Calculation");
				}

				if (mShikigamiSelectionOperator.GetSelectedShikigami() != null)
				{
					mCalculationResultFunction.MarkClean();
				}

				using (mPerformanceTracker.Track("Calculation.ShowResult"))
				{
					mCalculationResultFunction.ShowResult();
				}
			}
		}

		private void markCalculationResultDirty(string source = "Other")
		{
			mCalculationResultFunction.MarkDirty(source);
		}

		/****************************************************************************************************
		  式神データ再読み込み
		****************************************************************************************************/
		private void btnReLoad_Click(object sender, EventArgs e)
		{
			mShikigamiSelectionFunction.InitializeShikigamiComboBox();
			markCalculationResultDirty();
		}

		/****************************************************************************************************
		  初期化
		****************************************************************************************************/
		private void MainForm_Load(object sender, EventArgs e)
		{
			using (mPerformanceTracker.Track("MainForm.Load"))
			{
				using (mPerformanceTracker.Track("MainForm.InitializeInputControls"))
				{
					initializeInputControls();
				}

				using (mPerformanceTracker.Track("MainForm.InitializeComboBoxes"))
				{
					initializeComboBoxes();
				}

				using (mPerformanceTracker.Track("MainForm.RegisterCalculationInputChangedEvents"))
				{
					registerCalculationInputChangedEvents();
				}

				updateSaveButtonEnabled("Startup");
			}
		}

		private void initializeInputControls()
		{
			using (mPerformanceTracker.Track("MainForm.CreateMitamaSlotInputControls"))
			{
				mMitamaSlotInputControls = createMitamaSlotInputControls();
			}

			using (mPerformanceTracker.Track("MainForm.CreateSetEffectComboBoxes"))
			{
				mSetEffectComboBoxes = createSetEffectComboBoxes();
			}

			using (mPerformanceTracker.Track("MainForm.CreateUniqueEffectComboBoxes"))
			{
				mUniqueEffectComboBoxes = createUniqueEffectComboBoxes();
			}

			var mitamaInputControls = new MitamaInputControlSet(mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);

			mComboBoxOperator = new ComboBoxOperator(mPerformanceTracker);

			var inputOperator = new MitamaInputOperator(mitamaInputControls, mComboBoxOperator, mPerformanceTracker);

			mMitamaInputFunction = new MitamaInputFunction(inputOperator, mPerformanceTracker, markCalculationResultDirty, () => mOperationState == MainFormOperationState.LOADING);

			var shikigamiControls = new ShikigamiSelectionControlSet(cmbShikigami, cmbRarityFilter, txtBaseStats);

			mShikigamiSelectionOperator = new ShikigamiSelectionOperator(shikigamiControls, mComboBoxOperator, mPerformanceTracker);

			mShikigamiSelectionFunction = new ShikigamiSelectionFunction(mShikigamiSelectionOperator, mPerformanceTracker, markCalculationResultDirty);

			mCalculationFunction = new CalculationFunction(inputOperator, mShikigamiSelectionOperator, mPerformanceTracker, CalculationInputValidationErrorHandler.Handle);

			var calculationResultControls = new CalculationResultControlSet(txtMitamaOnly, txtFinalStats);

			var calculationResultOperator = new CalculationResultOperator(calculationResultControls);

			mCalculationResultFunction = new CalculationResultFunction(calculationResultOperator, mShikigamiSelectionOperator, () => mOperationState == MainFormOperationState.LOADING || mOperationState == MainFormOperationState.CLEARING, updateSaveButtonEnabled);

			var saveDataButtonControls = new SaveDataButtonControlSet(btnSave, btnLoad);

			var saveDataButtonOperator = new SaveDataButtonOperator(saveDataButtonControls);

			mSaveDataButtonFunction = new SaveDataButtonFunction(saveDataButtonOperator, inputOperator, mPerformanceTracker, () => mOperationState == MainFormOperationState.IDLE);

			mSaveDataSaveFunction = new SaveDataSaveFunction(inputOperator, mShikigamiSelectionOperator, mCalculationResultFunction);

			mSaveDataLoadFunction = new SaveDataLoadFunction(inputOperator, mShikigamiSelectionFunction, mPerformanceTracker);

			mSaveDataApplyFunction = new SaveDataApplyFunction(inputOperator, mShikigamiSelectionOperator, mShikigamiSelectionFunction, mPerformanceTracker);
		}

		private void initializeComboBoxes()
		{
			using (mPerformanceTracker.Track("MainForm.InitializeRarityFilterComboBox"))
			{
				mShikigamiSelectionFunction.InitializeRarityFilterComboBox(cmbRarityFilter_SelectedIndexChanged);
			}

			using (mPerformanceTracker.Track("MainForm.InitializeShikigamiComboBox"))
			{
				mShikigamiSelectionFunction.InitializeShikigamiComboBox();
			}

			mMitamaInputFunction.InitializeComboBoxes();
		}

		private void registerCalculationInputChangedEvents()
		{
			mMitamaInputFunction.RegisterInputChangedEvents();
		}

		private void btnSave_EnabledChanged(object sender, EventArgs e)
		{
			using (mPerformanceTracker.Track("SaveButton.SaveEnabledChanged"))
			{
			}
		}

		private void btnLoad_EnabledChanged(object sender, EventArgs e)
		{
			using (mPerformanceTracker.Track("SaveButton.LoadEnabledChanged"))
			{
			}
		}

		/****************************************************************************************************
		  メインステータス表示
		****************************************************************************************************/

		private void cmbMainStat1_SelectedIndexChanged(object sender, EventArgs e)
		{
			mMitamaInputFunction?.UpdateMainStatValue(1);
		}

		private void cmbMainStat2_SelectedIndexChanged(object sender, EventArgs e)
		{
			mMitamaInputFunction?.UpdateMainStatValue(2);
		}

		private void cmbMainStat3_SelectedIndexChanged(object sender, EventArgs e)
		{
			mMitamaInputFunction?.UpdateMainStatValue(3);
		}

		private void cmbMainStat4_SelectedIndexChanged(object sender, EventArgs e)
		{
			mMitamaInputFunction?.UpdateMainStatValue(4);
		}

		private void cmbMainStat5_SelectedIndexChanged(object sender, EventArgs e)
		{
			mMitamaInputFunction?.UpdateMainStatValue(5);
		}

		private void cmbMainStat6_SelectedIndexChanged(object sender, EventArgs e)
		{
			mMitamaInputFunction?.UpdateMainStatValue(6);
		}
		/****************************************************************************************************
		  SaveData保存
		****************************************************************************************************/

		private void btnSave_Click(object sender, EventArgs e)
		{
			using (var dialog = new SaveDataSaveDialog(mShikigamiSelectionOperator.GetShikigamiText(), mSaveDataSaveFunction.GetSaveLevel()))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				setOperationState(MainFormOperationState.SAVING);

				try
				{
					mSaveDataSaveFunction.Save(dialog.SelectedSaveType, dialog.FilePath);
				}
				finally
				{
					setOperationState(MainFormOperationState.IDLE);
				}
			}
		}

		private void updateSaveButtonEnabled(string source = "Other")
		{
			mSaveDataButtonFunction.Update(source);
		}

		/****************************************************************************************************
		  SaveData適用
		****************************************************************************************************/
		private void btnLoad_Click(object sender, EventArgs e)
		{
			using (var dialog = new SaveDataLoadDialog())
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				setOperationState(MainFormOperationState.LOADING);

				bool loadSucceeded = false;

				try
				{
					using (mPerformanceTracker.Track("SaveData.Load"))
					{
						string shikigamiName;

						MitamaSetInputModel inputModel = mSaveDataLoadFunction.Load(dialog.SelectedLoadType, dialog.FilePath, out shikigamiName);

						mSaveDataApplyFunction.Apply(dialog.SelectedLoadType, shikigamiName, inputModel);

						loadSucceeded = true;
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Operation=SaveData読み込み Message={ex}");

					MessageBox.Show(
						"SaveDataの読み込み中にエラーが発生しました。",
						"SaveData読み込み",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
				}
				finally
				{
					setOperationState(MainFormOperationState.IDLE);
				}

				if (loadSucceeded)
				{
					markCalculationResultDirty();
				}
			}
		}
		/****************************************************************************************************
		  計算結果比較
		****************************************************************************************************/
		private void btnCompareResult_Click(object sender, EventArgs e)
		{
			using (var dialog = new SnapshotCompareFileSelectDialog())
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				CalculationSnapshotSaveData baseSnapshot;
				CalculationSnapshotSaveData targetSnapshot;

				StatusComparisonResultDto comparisonResult;

				try
				{

					baseSnapshot = SaveDataAccess.LoadSnapshot(dialog.BaseSnapshotFilePath);
					targetSnapshot = SaveDataAccess.LoadSnapshot(dialog.TargetSnapshotFilePath);

					if (baseSnapshot == null || targetSnapshot == null)
					{
						Logger.Error("Operation=計算結果比較 Message=スナップショットデータの読み込み結果がnullでした。");

						MessageBox.Show(
							"スナップショットデータの読み込みに失敗しました。",
							"計算結果比較",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);

						return;
					}

					StatusDto baseStatus = StatusConverter.ToDto(baseSnapshot.FinalStatus);
					StatusDto targetStatus = StatusConverter.ToDto(targetSnapshot.FinalStatus);

					if (baseStatus == null || targetStatus == null)
					{
						return;
					}

					comparisonResult = ComparisonGateway.CompareStatus(baseStatus, targetStatus);
				}
				catch (Exception ex)
				{
					Logger.Error($"Operation=計算結果比較 Message={ex}");

					MessageBox.Show(
						"計算結果比較中に予期しないエラーが発生しました。",
						"計算結果比較",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);

					return;
				}

				string baseSnapshotName = baseSnapshot.SnapshotName;
				string targetSnapshotName = targetSnapshot.SnapshotName;

				using (var form = new StatusComparisonResultForm(baseSnapshotName, targetSnapshotName, comparisonResult))
				{
					form.ShowDialog(this);
				}
			}
		}

		/****************************************************************************************************
		  式神登録
		****************************************************************************************************/
		private void btnAddShikigami_Click(object sender, EventArgs e)
		{
			var selectedShikigami = cmbShikigami.SelectedItem as ShikigamiDto;

			using (var form = new ShikigamiRegisterForm())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					mShikigamiSelectionFunction.InitializeShikigamiComboBox();

					if (selectedShikigami != null)
					{
						mShikigamiSelectionFunction.ApplyShikigami(selectedShikigami.Name);
					}

					markCalculationResultDirty();
				}
			}
		}

		/****************************************************************************************************
		  式神編集
		****************************************************************************************************/
		private void btnEditShikigami_Click(object sender, EventArgs e)
		{
			var selectedShikigami = cmbShikigami.SelectedItem as ShikigamiDto;

			if (selectedShikigami == null)
			{
				MessageBox.Show(
					"編集する式神を選択してください。",
					"式神データ編集",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);

				return;
			}

			using (var form = new ShikigamiRegisterForm(selectedShikigami))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					mShikigamiSelectionFunction.InitializeShikigamiComboBox();

					if (form.EditedShikigami != null)
					{
						mShikigamiSelectionFunction.ApplyShikigami(form.EditedShikigami.Name);
						markCalculationResultDirty();
					}
				}
			}
		}

		/****************************************************************************************************
		  入力クリア
		****************************************************************************************************/
		private void btnClear_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(
				"入力内容と計算結果をクリアします。よろしいですか？",
				"確認",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
			{
				return;
			}

			setOperationState(MainFormOperationState.CLEARING);

			try
			{
				clearInputs();
				deleteLastCalculationResult();
			}
			finally
			{
				setOperationState(MainFormOperationState.IDLE);
			}

			markCalculationResultDirty();
		}

		// 起動直後と同じ入力状態に戻す
		private void clearInputs()
		{
			using (mPerformanceTracker.Track("MainForm.ClearInputs"))
			{
				mShikigamiSelectionOperator.ClearShikigamiSelection();
				mMitamaInputFunction.Clear();
				mCalculationResultFunction.ClearTexts();
			}
		}

		private void deleteLastCalculationResult()
		{
			mCalculationResultFunction.DeleteResult();

#if DEBUG
			mLastCalculationTestSource = null;
#endif
		}
		/****************************************************************************************************
		  結果表示フォーム
		****************************************************************************************************/
		private void btnResultView_Click(object sender, EventArgs e)
		{
			CalculationResultDto calculationResult = mCalculationResultFunction.LastCalculationResult;

			if (calculationResult == null)
			{
				MessageBox.Show(
					"先に計算を実行してください。",
					"確認",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);

				return;
			}

			using (var form = new ResultViewForm(calculationResult))
			{
				form.ShowDialog(this);
			}
		}

		/****************************************************************************************************
		  式神選択解除
		****************************************************************************************************/
		private void btnClearShikigami_Click(object sender, EventArgs e)
		{
			mShikigamiSelectionOperator.ClearShikigamiSelection();
		}

		/****************************************************************************************************
		  式神フィルター
		****************************************************************************************************/
		private void cmbRarityFilter_SelectedIndexChanged(object sender, EventArgs e)
		{
			mShikigamiSelectionFunction?.HandleRarityFilterSelectedIndexChanged();
		}

		/****************************************************************************************************
		  式神復旧
		****************************************************************************************************/
		private void btnRecoveryShikigami_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "復旧元の式神データを選択してください。";
				dialog.Filter = $"式神データ (*.csv)|*.csv";
				dialog.InitialDirectory = AppPath.DataBackupDirectoryPath;

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				List<ShikigamiDto> recoveryShikigamiList = new List<ShikigamiDto>();

				try
				{
					ShikigamiDataOutcomeDto outcome = ShikigamiGateway.GetRecoveryCandidateShikigamiList(AppPath.ShikigamiDataCsvPath, dialog.FileName, out recoveryShikigamiList);

					if (ShikigamiDataErrorHandler.Handle(outcome, "式神復旧候補読み込み"))
					{
						return;
					}
				}
				catch (Exception ex)
				{
					ShikigamiDataErrorHandler.Handle(ex, "式神復旧候補読み込み");
					return;
				}

				if (recoveryShikigamiList.Count == 0)
				{
					string message = "復旧できる式神はありませんでした。";

					MessageBox.Show(
						message,
						"式神復旧候補読み込み",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);

					Logger.Info(message);
					return;
				}

				using (var shikigamiRecoveryDialog = new ShikigamiRecoveryDialog(recoveryShikigamiList))
				{
					shikigamiRecoveryDialog.ShowDialog(this);

					if (shikigamiRecoveryDialog.DialogResult != DialogResult.OK)
					{
						return;
					}

					try
					{
						ShikigamiDataFileManager.CreateBackup();

						ShikigamiDataOutcomeDto outcome = ShikigamiGateway.RecoveryShikigami(AppPath.ShikigamiDataCsvPath, shikigamiRecoveryDialog.SelectedRecoveryCandidates);

						if (ShikigamiDataErrorHandler.Handle(outcome, "式神復旧"))
						{
							return;
						}
					}
					catch (Exception ex)
					{
						ShikigamiDataErrorHandler.Handle(ex, "式神復旧");
						return;
					}

					string message = "式神の復旧に成功しました。";

					MessageBox.Show(
						message,
						"式神復旧",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);

					Logger.Info(message);

					mShikigamiSelectionFunction.InitializeShikigamiComboBox();
					markCalculationResultDirty();
				}
			}
		}

		/****************************************************************************************************
		  終了処理ログ
		****************************************************************************************************/

		private void logShutdownEvent(string eventName)
		{
			try
			{
				Logger.Info($"Operation=Shutdown Event={eventName}");
			}
			catch
			{
				// 調査ログの出力失敗でフォーム終了を妨げない。
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			logShutdownEvent("OnFormClosing.Begin");

			try
			{
				base.OnFormClosing(e);
			}
			finally
			{
				logShutdownEvent("OnFormClosing.End");
			}
		}

		/****************************************************************************************************
		  性能ログ出力
		****************************************************************************************************/

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			logShutdownEvent("OnFormClosed.Begin");

			try
			{
				logShutdownEvent("PerformanceLogger.Write.Begin");

				PerformanceLogger.Write(mPerformanceSessionId, mPerformanceTracker.GetSnapshot());

				logShutdownEvent("PerformanceLogger.Write.End");
			}
			catch (Exception ex)
			{
				try
				{
					Logger.Error($"Operation=性能ログ出力 Message={ex}");
				}
				catch
				{
					// 性能ログの出力失敗でフォーム終了を妨げない。
				}
			}
			finally
			{
				logShutdownEvent("Base.OnFormClosed.Begin");

				try
				{
					base.OnFormClosed(e);
				}
				finally
				{
					logShutdownEvent("Base.OnFormClosed.End");
				}
			}
		}
	}
}
