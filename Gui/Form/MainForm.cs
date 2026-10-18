using Gui.Converter;
using Gui.Definition;
using Gui.Dialog;
using Gui.Factory;
using Gui.Form.Applicator;
using Gui.Form.Control;
using Gui.Formatter;
using Gui.Logging;
using Gui.Model;
using Gui.Resolver;
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
				new MitamaSlotInputControl
				{
					MainStatComboBox = cmbMainStat1,
					MainValueTextBox = txtMainVal1,
					SubStats = new SubStatInputControl[]
					{
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat11,
							ValueTextBox = txtSubVal11,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat21,
							ValueTextBox = txtSubVal21,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat31,
							ValueTextBox = txtSubVal31,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat41,
							ValueTextBox = txtSubVal41,
						}
					}
				},

				new MitamaSlotInputControl
				{
					MainStatComboBox = cmbMainStat2,
					MainValueTextBox = txtMainVal2,
					SubStats = new SubStatInputControl[]
					{
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat12,
							ValueTextBox = txtSubVal12,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat22,
							ValueTextBox = txtSubVal22,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat32,
							ValueTextBox = txtSubVal32,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat42,
							ValueTextBox = txtSubVal42,
						}
					}
				},

				new MitamaSlotInputControl
				{
					MainStatComboBox = cmbMainStat3,
					MainValueTextBox = txtMainVal3,
					SubStats = new SubStatInputControl[]
					{
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat13,
							ValueTextBox = txtSubVal13,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat23,
							ValueTextBox = txtSubVal23,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat33,
							ValueTextBox = txtSubVal33,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat43,
							ValueTextBox = txtSubVal43,
						}
					}
				},

				new MitamaSlotInputControl
				{
					MainStatComboBox = cmbMainStat4,
					MainValueTextBox = txtMainVal4,
					SubStats = new SubStatInputControl[]
					{
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat14,
							ValueTextBox = txtSubVal14,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat24,
							ValueTextBox = txtSubVal24,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat34,
							ValueTextBox = txtSubVal34,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat44,
							ValueTextBox = txtSubVal44,
						}
					}
				},

				new MitamaSlotInputControl
				{
					MainStatComboBox = cmbMainStat5,
					MainValueTextBox = txtMainVal5,
					SubStats = new SubStatInputControl[]
					{
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat15,
							ValueTextBox = txtSubVal15,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat25,
							ValueTextBox = txtSubVal25,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat35,
							ValueTextBox = txtSubVal35,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat45,
							ValueTextBox = txtSubVal45,
						}
					}
				},

				new MitamaSlotInputControl
				{
					MainStatComboBox = cmbMainStat6,
					MainValueTextBox = txtMainVal6,
					SubStats = new SubStatInputControl[]
					{
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat16,
							ValueTextBox = txtSubVal16,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat26,
							ValueTextBox = txtSubVal26,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat36,
							ValueTextBox = txtSubVal36,
						},
						new SubStatInputControl
						{
							TypeComboBox = cmbSubStat46,
							ValueTextBox = txtSubVal46,
						}
					}
				}
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
		CalculationTestSourceSaveData calculationTestSource;
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
			calculationTestSource = CalculationTestSourceConverter.ToSaveData(				baseStatus, mitamaSet, calculationResult);
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
			if (mOperationState == MainFormOperationState.LOADING)
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

			using (mPerformanceTracker.Track("MainForm.InitializeMainStatComboBoxes"))
			{
				initializeMainStatComboBoxes();
			}

			using (mPerformanceTracker.Track("MainForm.InitializeSubStatComboBoxes"))
			{
				initializeSubStatComboBoxes();
			}

			using (mPerformanceTracker.Track("MainForm.InitializeSetEffectComboBoxes"))
			{
				initializeSetEffectComboBoxes();
			}

			using (mPerformanceTracker.Track("MainForm.InitializeUniqueEffectComboBoxes"))
			{
				initializeUniqueEffectComboBoxes();
			}
		}

		private void initializeRarityFilterComboBox()
		{
			using (mPerformanceTracker.Track("RarityFilter.SetItems"))
			{
				setComboItems(
					cmbRarityFilter,
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

		private void initializeMainStatComboBoxes()
		{
			setComboItems(cmbMainStat1,
				DisplayText.ATTACK);

			setComboItems(cmbMainStat2,
				DisplayText.SPEED,
				DisplayText.ADDITIONAL_ATTACK_RATE,
				DisplayText.ADDITIONAL_HP_RATE,
				DisplayText.ADDITIONAL_DEFENSE_RATE);

			setComboItems(cmbMainStat3,
				DisplayText.DEFENSE);

			setComboItems(cmbMainStat4,
				DisplayText.EFFECT_HIT,
				DisplayText.EFFECT_RESIST,
				DisplayText.ADDITIONAL_ATTACK_RATE,
				DisplayText.ADDITIONAL_HP_RATE,
				DisplayText.ADDITIONAL_DEFENSE_RATE);

			setComboItems(cmbMainStat5,
				DisplayText.HP);

			setComboItems(cmbMainStat6,
				DisplayText.CRITICAL_RATE,
				DisplayText.CRITICAL_DAMAGE,
				DisplayText.ADDITIONAL_ATTACK_RATE,
				DisplayText.ADDITIONAL_HP_RATE,
				DisplayText.ADDITIONAL_DEFENSE_RATE);
		}

		private void initializeSubStatComboBoxes()
		{
			foreach (MitamaSlotInputControl slot in mMitamaSlotInputControls)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					using (mPerformanceTracker.Track("SubStat.SetItems"))
					{
						setComboItems(subStat.TypeComboBox,
							DisplayText.NONE,
							DisplayText.SPEED,
							DisplayText.ADDITIONAL_ATTACK_RATE,
							DisplayText.ADDITIONAL_HP_RATE,
							DisplayText.ADDITIONAL_DEFENSE_RATE,
							DisplayText.CRITICAL_RATE,
							DisplayText.CRITICAL_DAMAGE,
							DisplayText.EFFECT_HIT,
							DisplayText.EFFECT_RESIST,
							DisplayText.ATTACK,
							DisplayText.HP,
							DisplayText.DEFENSE);
					}
				}
			}
		}

		private void initializeSetEffectComboBoxes()
		{
			foreach (ComboBox comboBox in mSetEffectComboBoxes)
			{
				setComboItems(comboBox,
					DisplayText.NONE,
					DisplayText.CRITICAL_RATE,
					DisplayText.CRITICAL_DAMAGE,
					DisplayText.EFFECT_HIT,
					DisplayText.EFFECT_RESIST,
					DisplayText.ADDITIONAL_ATTACK_RATE,
					DisplayText.ADDITIONAL_HP_RATE,
					DisplayText.ADDITIONAL_DEFENSE_RATE);
			}
		}

		private void initializeUniqueEffectComboBoxes()
		{
			foreach (ComboBox comboBox in mUniqueEffectComboBoxes)
			{
				setComboItems(comboBox,
					DisplayText.NONE,
					DisplayText.CRITICAL_RATE,
					DisplayText.EFFECT_HIT,
					DisplayText.EFFECT_RESIST,
					DisplayText.ADDITIONAL_ATTACK_RATE,
					DisplayText.ADDITIONAL_HP_RATE,
					DisplayText.ADDITIONAL_DEFENSE_RATE);
			}
		}

		private void setComboItems(ComboBox comboBox, params string[] items)
		{
			if (comboBox == null || items == null)
			{
				return;
			}

			bool isRarityFilter = comboBox == cmbRarityFilter;
			bool isSubStat = mMitamaSlotInputControls != null &&
				mMitamaSlotInputControls.Any(slot =>
					slot.SubStats.Any(subStat => subStat.TypeComboBox == comboBox));

			string targetPrefix = isRarityFilter ? "RarityFilter" : isSubStat ? "SubStat" : null;

			using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.BeginUpdate") : null)
			{
				comboBox.BeginUpdate();
			}

			try
			{
				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.ClearItems") : null)
				{
					comboBox.Items.Clear();
				}

				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.AddRange") : null)
				{
					if (isSubStat)
					{
						string handleState = comboBox.IsHandleCreated ? "HandleCreated" : "HandleNotCreated";

						using (mPerformanceTracker.Track($"SubStat.AddRange.{handleState}"))
						{
							string configuration = $"Sorted{comboBox.Sorted}.{comboBox.DropDownStyle}";

							using (mPerformanceTracker.Track($"SubStat.AddRange.Configuration.{configuration}"))
							{
								comboBox.Items.AddRange(items);
							}
						}
					}
					else
					{
						comboBox.Items.AddRange(items);
					}
				}

				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.ResetSelection") : null)
				{
					comboBox.SelectedIndex = -1;
				}

				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.ClearText") : null)
				{
					comboBox.Text = "";
				}
			}
			finally
			{
				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.EndUpdate") : null)
				{
					comboBox.EndUpdate();
				}
			}
		}

		private void resetComboBox(ComboBox comboBox)
		{
			if (comboBox == null)
			{
				return;
			}

			comboBox.SelectedIndex = -1;
			comboBox.Text = "";
		}

		private void registerCalculationInputChangedEvents()
		{
			foreach (MitamaSlotInputControl slot in mMitamaSlotInputControls)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					subStat.TypeComboBox.SelectedIndexChanged += calculationInputChanged;
					subStat.ValueTextBox.TextChanged += calculationInputChanged;
				}
			}

			foreach (ComboBox comboBox in mSetEffectComboBoxes)
			{
				comboBox.SelectedIndexChanged += calculationInputChanged;
			}

			foreach (ComboBox comboBox in mUniqueEffectComboBoxes)
			{
				comboBox.SelectedIndexChanged += calculationInputChanged;
			}
		}

		private void calculationInputChanged(object sender, EventArgs e)
		{
			string source = "Input.Other";

			if (sender is System.Windows.Forms.Control control)
			{
				if (control.Name.StartsWith("cmbSubStat", StringComparison.Ordinal))
				{
					source = "Input.SubStatType";
				}
				else if (control.Name.StartsWith("txtSubVal", StringComparison.Ordinal))
				{
					source = "Input.SubStatValue";
				}
				else if (control.Name.StartsWith("cmbSetBonus", StringComparison.Ordinal))
				{
					source = "Input.SetEffect";
				}
				else if (control.Name.StartsWith("cmbUnique", StringComparison.Ordinal))
				{
					source = "Input.UniqueEffect";
				}
			}

			markCalculationResultDirty(source);
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
			updateMainStatValue(cmbMainStat1, txtMainVal1, 1);
		}

		private void cmbMainStat2_SelectedIndexChanged(object sender, EventArgs e)
		{
			updateMainStatValue(cmbMainStat2, txtMainVal2, 2);
		}

		private void cmbMainStat3_SelectedIndexChanged(object sender, EventArgs e)
		{
			updateMainStatValue(cmbMainStat3, txtMainVal3, 3);
		}

		private void cmbMainStat4_SelectedIndexChanged(object sender, EventArgs e)
		{
			updateMainStatValue(cmbMainStat4, txtMainVal4, 4);
		}

		private void cmbMainStat5_SelectedIndexChanged(object sender, EventArgs e)
		{
			updateMainStatValue(cmbMainStat5, txtMainVal5, 5);
		}

		private void cmbMainStat6_SelectedIndexChanged(object sender, EventArgs e)
		{
			updateMainStatValue(cmbMainStat6, txtMainVal6, 6);
		}

		private void updateMainStatValue(ComboBox comboBox, TextBox textBox, int slot)
		{
			string statText = comboBox.SelectedItem?.ToString();

			if (string.IsNullOrEmpty(statText))
			{
				textBox.Text = "";
			}
			else
			{
				double value = MainStatValueResolver.Resolve(statText, slot);
				textBox.Text = value.ToString();
			}

			markCalculationResultDirty("Input.MainStat");
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
					if (dialog.SelectedLoadType == SaveDataLoadType.Build)
					{
						BuildSaveData data = SaveDataAccess.LoadBuild(dialog.FilePath);

						if (data == null)
						{
							throw new InvalidOperationException("Build save data is null.");
						}

						MitamaSetInputModel inputModel = MitamaSetConverter.ToInputModel(data.MitamaSet);

						if (inputModel == null)
						{
							throw new InvalidOperationException("MitamaSet input model is null.");
						}

						validateBuildShikigami(data.ShikigamiName);

						SaveDataInputModelValidator.Validate(
							inputModel,
							mMitamaSlotInputControls,
							mSetEffectComboBoxes,
							mUniqueEffectComboBoxes);

						applySaveDataWithRollback(() => applyBuildSaveDataToUI(data.ShikigamiName, inputModel));
					}
					else if (dialog.SelectedLoadType == SaveDataLoadType.MitamaSet)
					{
						MitamaSetSaveData data = SaveDataAccess.LoadMitamaSet(dialog.FilePath);
						MitamaSetInputModel inputModel = MitamaSetConverter.ToInputModel(data);

						if (inputModel == null)
						{
							throw new InvalidOperationException("MitamaSet input model is null.");
						}

						SaveDataInputModelValidator.Validate(
							inputModel,
							mMitamaSlotInputControls,
							mSetEffectComboBoxes,
							mUniqueEffectComboBoxes);

						applySaveDataWithRollback(() =>
						{
							using (mPerformanceTracker.Track("SaveData.ApplyMitamaSet"))
							{
								MitamaSetInputModelApplicator.Apply(
									inputModel,
									mMitamaSlotInputControls,
									mSetEffectComboBoxes,
									mUniqueEffectComboBoxes);
							}
						});
					}
					else
					{
						throw new InvalidOperationException($"Unsupported SaveData load type: {dialog.SelectedLoadType}");
					}

					loadSucceeded = true;
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
					mUniqueEffectComboBoxes);
			}
		}

		private void applySaveDataWithRollback(Action apply)
		{
			MitamaSetInputModel previousInputModel = MitamaSetInputModelFactory.Create(mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);

			string previousShikigamiName = (cmbShikigami.SelectedItem as ShikigamiDto)?.Name;
			object previousRarity = cmbRarityFilter.SelectedItem;

			try
			{
				apply();
			}
			catch
			{
				try
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
						mUniqueEffectComboBoxes);

					Logger.Info("Operation=SaveDataロールバック Message=UI状態を復元しました。");
				}
				catch (Exception rollbackEx)
				{
					Logger.Error($"Operation=SaveDataロールバック Message={rollbackEx}");
				}

				throw;
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

			clearInputs();

			deleteLastCalculationResult();
			markCalculationResultDirty();
		}

		// 起動直後と同じ入力状態に戻す
		private void clearInputs()
		{
			using (mPerformanceTracker.Track("MainForm.ClearInputs"))
			{
				clearShikigamiSelection();
				clearMitamaInputs();
				clearEffectInputs();
				clearResultTexts();
			}
		}

		private void clearShikigamiSelection()
		{
			cmbShikigami.SelectedIndex = -1;
		}

		private void clearMitamaInputs()
		{
			resetMainStatComboBoxes();
			resetSubStatComboBoxes();

			clearMainValueTextBoxes();
			clearSubValueTextBoxes();
		}

		private void resetMainStatComboBoxes()
		{
			foreach (MitamaSlotInputControl slot in mMitamaSlotInputControls)
			{
				resetComboBox(slot.MainStatComboBox);
			}
		}

		private void resetSubStatComboBoxes()
		{
			foreach (MitamaSlotInputControl slot in mMitamaSlotInputControls)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					resetComboBox(subStat.TypeComboBox);
				}
			}
		}

		private void clearMainValueTextBoxes()
		{
			foreach (MitamaSlotInputControl slot in mMitamaSlotInputControls)
			{
				slot.MainValueTextBox.Text = "";
			}
		}

		private void clearSubValueTextBoxes()
		{
			foreach (MitamaSlotInputControl slot in mMitamaSlotInputControls)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					subStat.ValueTextBox.Text = "";
				}
			}
		}

		private void clearEffectInputs()
		{
			resetEffectComboBoxes();
		}

		private void resetEffectComboBoxes()
		{
			foreach (ComboBox comboBox in mSetEffectComboBoxes)
			{
				resetComboBox(comboBox);
			}

			foreach (ComboBox comboBox in mUniqueEffectComboBoxes)
			{
				resetComboBox(comboBox);
			}
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
		  性能ログ出力
		****************************************************************************************************/

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			try
			{
				PerformanceLogger.Write(mPerformanceSessionId, mPerformanceTracker.GetSnapshot());
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
				base.OnFormClosed(e);
			}
		}
	}
}
