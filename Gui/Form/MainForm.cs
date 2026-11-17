using Gui.ControlSet;
using Gui.Converter;
using Gui.Definition;
using Gui.Dialog;
using Gui.Factory;
using Gui.Form.Applicator;
using Gui.Form.Control;
using Gui.Formatter;
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
using System.Linq;
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

		private readonly Guid mPerformanceSessionId = Guid.NewGuid();

		private MainFormOperationState mOperationState = MainFormOperationState.IDLE;

		private CalculationResultDto mLastCalculationResult = null;

		private bool mIsCalculationResultDirty = true;

		private List<ShikigamiDto> mShikigamiList = null;

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
			using (mPerformanceTracker.Track("Shikigami.SelectedIndexChanged"))
			{
				ShikigamiDto selected = cmbShikigami.SelectedItem as ShikigamiDto;

				using (mPerformanceTracker.Track("Shikigami.FormatBaseSummary"))
				{
					txtBaseStats.Text = StatusFormatter.FormatBaseSummary(selected?.Status);
				}

				using (mPerformanceTracker.Track("Shikigami.MarkCalculationResultDirty"))
				{
					markCalculationResultDirty("Input.Shikigami");
				}
			}
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
					MitamaSetInputModel inputModel;

					using (mPerformanceTracker.Track("Calculation.CreateInputModel"))
					{
						inputModel = MitamaSetInputModelFactory.Create(mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);
					}

					CalculationInputValidationOutcome validationOutcome;

					using (mPerformanceTracker.Track("Calculation.Validate"))
					{
						validationOutcome = CalculationInputValidator.Validate(inputModel);
					}

					if (CalculationInputValidationErrorHandler.Handle(validationOutcome))
					{
						return;
					}

					StatusDto baseStatus;
					MitamaSetDto mitamaSet;

					using (mPerformanceTracker.Track("Calculation.ConvertInput"))
					{
						baseStatus = getSelectedShikigamiStatus();
						mitamaSet = MitamaSetConverter.ToDto(inputModel);
					}

					using (mPerformanceTracker.Track("Calculation.Gateway"))
					{
						calculationResult = CalculationGateway.Calculate(baseStatus, mitamaSet);
					}

#if DEBUG
					calculationTestSource = CalculationTestSourceConverter.ToSaveData(baseStatus, mitamaSet, calculationResult);
#endif
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

				mLastCalculationResult = calculationResult;

#if DEBUG
				mLastCalculationTestSource = calculationTestSource;
#endif

				using (mPerformanceTracker.Track("Calculation.UpdateSaveButton"))
				{
					updateSaveButtonEnabled("Calculation");
				}

				if (cmbShikigami.SelectedItem != null)
				{
					markCalculationResultClean();
				}

				using (mPerformanceTracker.Track("Calculation.ShowResult"))
				{
					showCalculationResult(mLastCalculationResult);
				}
			}
		}

		private void markCalculationResultClean()
		{
			mIsCalculationResultDirty = false;
		}

		private void markCalculationResultDirty(string source = "Other")
		{
			if (mOperationState == MainFormOperationState.LOADING
				|| mOperationState == MainFormOperationState.CLEARING)
			{
				return;
			}

			updateSaveButtonEnabled(source);

			if (mIsCalculationResultDirty)
			{
				return;
			}

			mIsCalculationResultDirty = true;

			Logger.Info("Operation=ステータス計算結果状態変更 Message=入力内容が変更されたため、計算結果をDirty状態に変更しました。");
		}

		private StatusDto getSelectedShikigamiStatus()
		{
			var selected = cmbShikigami.SelectedItem as ShikigamiDto;

			return selected?.Status ?? new StatusDto();
		}

		/****************************************************************************************************
		  計算結果表示
		****************************************************************************************************/
		private void showCalculationResult(CalculationResultDto result)
		{
			if (result == null)
			{
				return;
			}

			txtMitamaOnly.Text = StatusFormatter.FormatMitamaSummary(result.MitamaOnlyStatus);

			if (cmbShikigami.SelectedItem != null)
			{
				txtFinalStats.Text = StatusFormatter.FormatFinalSummary(result.FinalStatus);
			}
			else
			{
				txtFinalStats.Text = "";
			}
		}

		/****************************************************************************************************
		  式神データ再読み込み
		****************************************************************************************************/
		private void btnReLoad_Click(object sender, EventArgs e)
		{
			initializeShikigamiComboBox();
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

			var controls = new MitamaInputControlSet(mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);

			mComboBoxOperator = new ComboBoxOperator(mPerformanceTracker);

			var inputOperator = new MitamaInputOperator(controls, mComboBoxOperator, mPerformanceTracker);

			mMitamaInputFunction = new MitamaInputFunction(inputOperator, mPerformanceTracker, markCalculationResultDirty, () => mOperationState == MainFormOperationState.LOADING);
		}

		private void initializeComboBoxes()
		{
			using (mPerformanceTracker.Track("MainForm.InitializeRarityFilterComboBox"))
			{
				initializeRarityFilterComboBox();
			}

			using (mPerformanceTracker.Track("MainForm.InitializeShikigamiComboBox"))
			{
				initializeShikigamiComboBox();
			}

			mMitamaInputFunction.InitializeComboBoxes();
		}

		private void initializeRarityFilterComboBox()
		{
			using (mPerformanceTracker.Track("RarityFilter.SetItems"))
			{
				mComboBoxOperator.SetItems(
					cmbRarityFilter,
					"RarityFilter",
					DisplayText.RARITY_ALL,
					DisplayText.RARITY_UR,
					DisplayText.RARITY_SP,
					DisplayText.RARITY_SSR,
					DisplayText.RARITY_SR);
			}

			cmbRarityFilter.SelectedIndexChanged -= cmbRarityFilter_SelectedIndexChanged;

			try
			{
				using (mPerformanceTracker.Track("RarityFilter.SelectAllWithoutHandler"))
				{
					cmbRarityFilter.SelectedIndex = 0;
				}
			}
			finally
			{
				cmbRarityFilter.SelectedIndexChanged += cmbRarityFilter_SelectedIndexChanged;
			}

			using (mPerformanceTracker.Track("RarityFilter.ApplyFilterExplicit"))
			{
				applyShikigamiFilter();
			}
		}

		private void initializeShikigamiComboBox()
		{
			List<ShikigamiDto> shikigamiList;

			try
			{
				using (mPerformanceTracker.Track("Shikigami.LoadData"))
				{
					var outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out shikigamiList);

					if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ読み込み"))
					{
						ShikigamiDataFileManager.MoveBrokenFile();
						ShikigamiDataFileManager.RestoreDefaultIfMissing();

						outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out shikigamiList);

						if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ復元後読み込み"))
						{
							shikigamiList = new List<ShikigamiDto>();
						}
					}
				}
			}
			catch (Exception ex)
			{
				ShikigamiDataErrorHandler.Handle(ex, "式神データ読み込み");

				if (mShikigamiList == null)
				{
					mShikigamiList = new List<ShikigamiDto>();
				}

				return;
			}

			mShikigamiList = shikigamiList;

			using (mPerformanceTracker.Track("Shikigami.SetDisplayMember"))
			{
				cmbShikigami.DisplayMember = "Name";
			}

			using (mPerformanceTracker.Track("Shikigami.SetItems"))
			{
				setShikigamiComboBoxItems(mShikigamiList);
			}

			using (mPerformanceTracker.Track("Shikigami.ResetSelection"))
			{
				cmbShikigami.SelectedIndex = -1;
				txtBaseStats.Text = "";
			}
		}

		private void setShikigamiComboBoxItems(List<ShikigamiDto> shikigamiList)
		{
			cmbShikigami.BeginUpdate();

			try
			{
				cmbShikigami.Items.Clear();

				if (shikigamiList != null)
				{
					cmbShikigami.Items.AddRange(shikigamiList.ToArray());
				}
			}
			finally
			{
				cmbShikigami.EndUpdate();
			}
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
			using (var dialog = new SaveDataSaveDialog(cmbShikigami.Text, getSaveDataSaveLevel()))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				setOperationState(MainFormOperationState.SAVING);

				try
				{
					var inputModel = MitamaSetInputModelFactory.Create(mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);

					if (dialog.SelectedSaveType == SaveDataSaveType.Build)
					{
						BuildSaveData data = new BuildSaveData
						{
							MitamaSet = MitamaSetConverter.ToSaveData(inputModel),
							ShikigamiName = cmbShikigami.Text
						};

						SaveDataAccess.SaveBuild(dialog.FilePath, data);
					}
					else if (dialog.SelectedSaveType == SaveDataSaveType.MitamaSet)
					{
						var data = MitamaSetConverter.ToSaveData(inputModel);
						SaveDataAccess.SaveMitamaSet(dialog.FilePath, data);
					}
					else if (dialog.SelectedSaveType == SaveDataSaveType.CalculationSnapshot)
					{
						CalculationSnapshotSaveData data = new CalculationSnapshotSaveData
						{
							SnapshotName = createSnapshotNameFromFilePath(dialog.FilePath),
							CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
							ShikigamiName = cmbShikigami.Text,
							MitamaSet = MitamaSetConverter.ToSaveData(inputModel),
							MitamaStatus = StatusConverter.ToSaveData(mLastCalculationResult.MitamaOnlyStatus),
							FinalStatus = StatusConverter.ToSaveData(mLastCalculationResult.FinalStatus)
						};

						SaveDataAccess.SaveSnapshot(dialog.FilePath, data);
					}
				}
				finally
				{
					setOperationState(MainFormOperationState.IDLE);
				}
			}
		}

		private SaveDataSaveLevel getSaveDataSaveLevel()
		{
			if (canSaveCalculationSnapshot())
			{
				return SaveDataSaveLevel.SNAPSHOT_AVAILABLE;
			}

			if (cmbShikigami.SelectedItem != null)
			{
				return SaveDataSaveLevel.BUILD_AVAILABLE;
			}

			return SaveDataSaveLevel.MITAMA_SET_ONLY;
		}

		private void updateSaveButtonEnabled(string source = "Other")
		{
			using (mPerformanceTracker.Track("MainForm.UpdateSaveButtonEnabled"))
			using (mPerformanceTracker.Track($"SaveButton.Update.Source.{source}"))
			{
				bool isIdle = mOperationState == MainFormOperationState.IDLE;

				using (mPerformanceTracker.Track("SaveButton.SetLoadEnabled"))
				{
					btnLoad.Enabled = isIdle;
				}

				if (!isIdle)
				{
					using (mPerformanceTracker.Track("SaveButton.SetSaveEnabled"))
					{
						btnSave.Enabled = false;
					}

					return;
				}

				MitamaSetInputModel inputModel;

				using (mPerformanceTracker.Track("SaveButton.CreateInputModel"))
				{
					inputModel = MitamaSetInputModelFactory.Create(
						mMitamaSlotInputControls,
						mSetEffectComboBoxes,
						mUniqueEffectComboBoxes);
				}

				CalculationInputValidationOutcome validationOutcome;

				using (mPerformanceTracker.Track("SaveButton.Validate"))
				{
					validationOutcome = CalculationInputValidator.Validate(inputModel);
				}

				using (mPerformanceTracker.Track("SaveButton.SetSaveEnabled"))
				{
					btnSave.Enabled = validationOutcome != CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA;
				}
			}
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
						if (dialog.SelectedLoadType == SaveDataLoadType.Build)
						{
							BuildSaveData data;

							using (mPerformanceTracker.Track("SaveData.LoadFile"))
							{
								data = SaveDataAccess.LoadBuild(dialog.FilePath, mPerformanceTracker.Track);
							}

							if (data == null)
							{
								throw new InvalidOperationException("Build save data is null.");
							}

							MitamaSetInputModel inputModel;

							using (mPerformanceTracker.Track("SaveData.Convert"))
							{
								inputModel = MitamaSetConverter.ToInputModel(data.MitamaSet);
							}

							if (inputModel == null)
							{
								throw new InvalidOperationException("MitamaSet input model is null.");
							}

							using (mPerformanceTracker.Track("SaveData.Validate"))
							{
								validateBuildShikigami(data.ShikigamiName);

								SaveDataInputModelValidator.Validate(
									inputModel,
									mMitamaSlotInputControls,
									mSetEffectComboBoxes,
									mUniqueEffectComboBoxes);
							}

							applySaveDataWithRollback(() => applyBuildSaveDataToUI(data.ShikigamiName, inputModel));
						}
						else if (dialog.SelectedLoadType == SaveDataLoadType.MitamaSet)
						{
							MitamaSetSaveData data;

							using (mPerformanceTracker.Track("SaveData.LoadFile"))
							{
								data = SaveDataAccess.LoadMitamaSet(dialog.FilePath, mPerformanceTracker.Track);
							}

							MitamaSetInputModel inputModel;

							using (mPerformanceTracker.Track("SaveData.Convert"))
							{
								inputModel = MitamaSetConverter.ToInputModel(data);
							}

							if (inputModel == null)
							{
								throw new InvalidOperationException("MitamaSet input model is null.");
							}

							using (mPerformanceTracker.Track("SaveData.Validate"))
							{
								SaveDataInputModelValidator.Validate(
									inputModel,
									mMitamaSlotInputControls,
									mSetEffectComboBoxes,
									mUniqueEffectComboBoxes);
							}

							applySaveDataWithRollback(() =>
							{
								using (mPerformanceTracker.Track("SaveData.ApplyMitamaSet"))
								{
									MitamaSetInputModelApplicator.Apply(
										inputModel,
										mMitamaSlotInputControls,
										mSetEffectComboBoxes,
										mUniqueEffectComboBoxes,
										mPerformanceTracker);
								}
							});
						}
						else
						{
							throw new InvalidOperationException($"Unsupported SaveData load type: {dialog.SelectedLoadType}");
						}

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

		private void validateBuildShikigami(string shikigamiName)
		{
			if (string.IsNullOrEmpty(shikigamiName))
			{
				return;
			}

			if (mShikigamiList == null || !mShikigamiList.Any(x => x != null && x.Name == shikigamiName))
			{
				throw new InvalidOperationException($"Shikigami not found: {shikigamiName}");
			}
		}

		private void applyBuildSaveDataToUI(string shikigamiName, MitamaSetInputModel inputModel)
		{
			using (mPerformanceTracker.Track("SaveData.ApplyBuild"))
			{
				applyShikigami(shikigamiName);

				MitamaSetInputModelApplicator.Apply(
					inputModel,
					mMitamaSlotInputControls,
					mSetEffectComboBoxes,
					mUniqueEffectComboBoxes,
					mPerformanceTracker);
			}
		}

		private void applySaveDataWithRollback(Action apply)
		{
			using (mPerformanceTracker.Track("SaveData.ApplyWithRollback"))
			{
				MitamaSetInputModel previousInputModel;

				using (mPerformanceTracker.Track("SaveData.RollbackSnapshot"))
				{
					previousInputModel = MitamaSetInputModelFactory.Create(
						mMitamaSlotInputControls,
						mSetEffectComboBoxes,
						mUniqueEffectComboBoxes);
				}

				string previousShikigamiName = (cmbShikigami.SelectedItem as ShikigamiDto)?.Name;
				object previousRarity = cmbRarityFilter.SelectedItem;

				try
				{
					using (mPerformanceTracker.Track("SaveData.ApplyUI"))
					{
						apply();
					}
				}
				catch
				{
					try
					{
						using (mPerformanceTracker.Track("SaveData.RollbackRestore"))
						{
							cmbRarityFilter.SelectedItem = previousRarity;

							if (string.IsNullOrEmpty(previousShikigamiName))
							{
								cmbShikigami.SelectedIndex = -1;
							}
							else if (!trySelectShikigami(previousShikigamiName))
							{
								throw new InvalidOperationException($"Failed to restore Shikigami: {previousShikigamiName}");
							}

							MitamaSetInputModelApplicator.Apply(
								previousInputModel,
								mMitamaSlotInputControls,
								mSetEffectComboBoxes,
								mUniqueEffectComboBoxes,
								mPerformanceTracker);

							Logger.Info("Operation=SaveDataロールバック Message=UI状態を復元しました。");
						}
					}
					catch (Exception rollbackEx)
					{
						Logger.Error($"Operation=SaveDataロールバック Message={rollbackEx}");
					}

					throw;
				}
			}
		}

		private void applyShikigami(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return;
			}

			if (trySelectShikigami(name))
			{
				return;
			}

			cmbRarityFilter.SelectedItem = DisplayText.RARITY_ALL;

			if (trySelectShikigami(name))
			{
				return;
			}

			throw new InvalidOperationException($"Failed to select Shikigami: {name}");
		}

		private bool trySelectShikigami(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			foreach (var item in cmbShikigami.Items)
			{
				var shikigami = item as ShikigamiDto;

				if (shikigami != null && shikigami.Name == name)
				{
					cmbShikigami.SelectedItem = shikigami;
					return true;
				}
			}

			return false;
		}

		/****************************************************************************************************
		  計算結果スナップショット保存
		****************************************************************************************************/
		private bool canSaveCalculationSnapshot()
		{
			return mLastCalculationResult != null && !mIsCalculationResultDirty;
		}

		private string createSnapshotNameFromFilePath(string filePath)
		{
			string fileName = Path.GetFileName(filePath);

			if (fileName.EndsWith(SaveDataFileDefinition.SNAPSHOT_EXTENSION))
			{
				return fileName.Substring(0, fileName.Length - SaveDataFileDefinition.SNAPSHOT_EXTENSION.Length);
			}

			return Path.GetFileNameWithoutExtension(filePath);
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
					initializeShikigamiComboBox();

					if (selectedShikigami != null)
					{
						applyShikigami(selectedShikigami.Name);
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
					initializeShikigamiComboBox();

					if (form.EditedShikigami != null)
					{
						applyShikigami(form.EditedShikigami.Name);
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
				clearShikigamiSelection();
				mMitamaInputFunction.Clear();
				clearResultTexts();
			}
		}

		private void clearShikigamiSelection()
		{
			cmbShikigami.SelectedIndex = -1;
		}

		private void clearResultTexts()
		{
			txtMitamaOnly.Text = "";
			txtFinalStats.Text = "";
		}

		private void deleteLastCalculationResult()
		{

			mLastCalculationResult = null;

#if DEBUG
			mLastCalculationTestSource = null;
#endif
		}

		/****************************************************************************************************
		  結果表示フォーム
		****************************************************************************************************/
		private void btnResultView_Click(object sender, EventArgs e)
		{
			if (mLastCalculationResult == null)
			{
				MessageBox.Show(
					"先に計算を実行してください。",
					"確認",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			using (var form = new ResultViewForm(mLastCalculationResult))
			{
				form.ShowDialog(this);
			}
		}

		/****************************************************************************************************
		  式神選択解除
		****************************************************************************************************/
		private void btnClearShikigami_Click(object sender, EventArgs e)
		{
			clearShikigamiSelection();
		}

		/****************************************************************************************************
		  式神フィルター
		****************************************************************************************************/
		private void cmbRarityFilter_SelectedIndexChanged(object sender, EventArgs e)
		{
			using (mPerformanceTracker.Track("RarityFilter.SelectedIndexChanged"))
			{
				applyShikigamiFilter();
			}
		}

		private void applyShikigamiFilter()
		{
			using (mPerformanceTracker.Track("RarityFilter.ApplyFilter"))
			{
				var selectedShikigami = cmbShikigami.SelectedItem as ShikigamiDto;
				string selectedShikigamiName = selectedShikigami?.Name;

				string selectedRarity = cmbRarityFilter.Text;

				var filteredList = mShikigamiList;

				if (selectedRarity != DisplayText.RARITY_ALL)
				{
					filteredList = mShikigamiList.Where(x => x.Rarity == selectedRarity).ToList();
				}

				using (mPerformanceTracker.Track("RarityFilter.SetShikigamiItems"))
				{
					setShikigamiComboBoxItems(filteredList);
				}

				cmbShikigami.SelectedIndex = -1;

				if (!string.IsNullOrEmpty(selectedShikigamiName))
				{
					trySelectShikigami(selectedShikigamiName);
				}
			}
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

					initializeShikigamiComboBox();
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
