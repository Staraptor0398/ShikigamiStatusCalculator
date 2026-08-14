using Gui.Common;
using Gui.Converter;
using Gui.Dialog;
using Gui.Factory;
using Gui.Form.Applicator;
using Gui.Form.Control;
using Gui.Formatter;
using Gui.Resolver;
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
			InitializeComponent();

#if DEBUG
			initializeDevelopmentControls();
#endif

			string version = loadAppVersion();
			this.Text = $"{this.Text} {version}";
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
		  式神ステータス表示
		****************************************************************************************************/
		private void cmbShikigami_SelectedIndexChanged(object sender, EventArgs e)
		{
			ShikigamiDto selected = cmbShikigami.SelectedItem as ShikigamiDto;

			txtBaseStats.Text = StatusFormatter.FormatBaseSummary(selected?.Status);

			markCalculationResultDirty();
		}

		/****************************************************************************************************
		  ステータス計算
		****************************************************************************************************/
		private void btnCalc_Click(object sender, EventArgs e)
		{
			var inputModel = MitamaSetInputModelFactory.Create(mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);

			CalculationInputValidationOutcome validationOutcome = CalculationInputValidator.Validate(inputModel);

			if (CalculationInputValidationErrorHandler.Handle(validationOutcome))
			{
				return;
			}

			var baseStatus = getSelectedShikigamiStatus();

			var mitamaSet = MitamaSetConverter.ToDto(inputModel);

			try
			{
				mLastCalculationResult = CalculationGateway.Calculate(baseStatus, mitamaSet);

#if DEBUG
				mLastCalculationTestSource = CalculationTestSourceConverter.ToSaveData(baseStatus, mitamaSet, mLastCalculationResult);
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

			updateSaveButtonEnabled();

			if (cmbShikigami.SelectedItem != null)
			{
				markCalculationResultClean();
			}

			showCalculationResult(mLastCalculationResult);
		}

		private void markCalculationResultClean()
		{
			mIsCalculationResultDirty = false;
		}

		private void markCalculationResultDirty()
		{
			updateSaveButtonEnabled();

			if (mLastCalculationResult == null)
			{
				return;
			}

			if (mIsCalculationResultDirty)
			{
				return;
			}

			mIsCalculationResultDirty = true;

			Logger.Info("Operation=ステータス計算結果状態変更 Message=入力内容が変更されたため、前回の計算結果を無効化しました。");
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
			initializeInputControls();

			initializeComboBoxes();
			registerCalculationInputChangedEvents();
		}

		private void initializeInputControls()
		{
			mMitamaSlotInputControls = createMitamaSlotInputControls();
			mSetEffectComboBoxes = createSetEffectComboBoxes();
			mUniqueEffectComboBoxes = createUniqueEffectComboBoxes();
		}

		private void initializeComboBoxes()
		{
			initializeRarityFilterComboBox();

			initializeShikigamiComboBox();
			initializeMainStatComboBoxes();
			initializeSubStatComboBoxes();
			initializeSetEffectComboBoxes();
			initializeUniqueEffectComboBoxes();
		}

		private void initializeRarityFilterComboBox()
		{
			setComboItems(
				cmbRarityFilter,
				DisplayText.RARITY_ALL,
				DisplayText.RARITY_UR,
				DisplayText.RARITY_SP,
				DisplayText.RARITY_SSR,
				DisplayText.RARITY_SR);

			cmbRarityFilter.SelectedIndex = 0;
		}

		private void initializeShikigamiComboBox()
		{
			var outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out mShikigamiList);

			if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ読み込み"))
			{
				ShikigamiDataFileManager.MoveBrokenFile();
				ShikigamiDataFileManager.RestoreDefaultIfMissing();

				outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out mShikigamiList);

				if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ復元後読み込み"))
				{
					mShikigamiList = new List<ShikigamiDto>();

				}
			}

			cmbShikigami.DataSource = mShikigamiList;
			cmbShikigami.DisplayMember = "Name";

			cmbShikigami.SelectedIndex = -1;
			txtBaseStats.Text = "";
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

			comboBox.Items.Clear();

			foreach (string item in items)
			{
				comboBox.Items.Add(item);
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
			markCalculationResultDirty();
		}

		/****************************************************************************************************
		  メインステータス表示
		****************************************************************************************************/
		private void cmbMainStat1_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = MainStatValueResolver.Resolve(cmbMainStat1.SelectedItem.ToString(), 1);
			txtMainVal1.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat2_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = MainStatValueResolver.Resolve(cmbMainStat2.SelectedItem.ToString(), 2);
			txtMainVal2.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat3_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = MainStatValueResolver.Resolve(cmbMainStat3.SelectedItem.ToString(), 3);
			txtMainVal3.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat4_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = MainStatValueResolver.Resolve(cmbMainStat4.SelectedItem.ToString(), 4);
			txtMainVal4.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat5_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = MainStatValueResolver.Resolve(cmbMainStat5.SelectedItem.ToString(), 5);
			txtMainVal5.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat6_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = MainStatValueResolver.Resolve(cmbMainStat6.SelectedItem.ToString(), 6);
			txtMainVal6.Text = value.ToString();

			markCalculationResultDirty();
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

				setSaveDataOperationButtonsEnabled(false);

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
						string snapshotName = createSnapshotNameFromFilePath(dialog.FilePath);

						var data = CalculationSnapshotSaveDataFactory.Create(cmbShikigami, mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes, snapshotName, mLastCalculationResult);
						SaveDataAccess.SaveSnapshot(dialog.FilePath, data);
					}
				}
				finally
				{
					setSaveDataOperationButtonsEnabled(true);
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

		private void updateSaveButtonEnabled()
		{
			var inputModel = MitamaSetInputModelFactory.Create(mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);
			btnSave.Enabled = CalculationInputValidator.Validate(inputModel) != CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA;
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

				setSaveDataOperationButtonsEnabled(false);

				try
				{
					if (dialog.SelectedLoadType == SaveDataLoadType.Build)
					{
						var data = SaveDataAccess.LoadBuild(dialog.FilePath);
						applyBuildSaveDataToUI(data);
					}
					else if (dialog.SelectedLoadType == SaveDataLoadType.MitamaSet)
					{
						var data = SaveDataAccess.LoadMitamaSet(dialog.FilePath);
						MitamaSetSaveDataApplicator.Apply(data, mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);
					}
				}
				finally
				{
					setSaveDataOperationButtonsEnabled(true);
					markCalculationResultDirty();
				}
			}
		}

		private void applyBuildSaveDataToUI(BuildSaveData data)
		{
			if (data == null)
			{
				return;
			}

			applyShikigami(data.ShikigamiName);
			MitamaSetSaveDataApplicator.Apply(data.MitamaSet, mMitamaSlotInputControls, mSetEffectComboBoxes, mUniqueEffectComboBoxes);
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

			MessageBox.Show($"式神が見つかりません： {name}");
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

		private void setSaveDataOperationButtonsEnabled(bool enabled)
		{
			btnSave.Enabled = enabled;
			btnLoad.Enabled = enabled;
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
			markCalculationResultDirty();
		}

		// 起動直後と同じ入力状態に戻す
		private void clearInputs()
		{
			clearShikigamiSelection();
			clearMitamaInputs();
			clearEffectInputs();
			clearResultTexts();
		}

		private void clearShikigamiSelection()
		{
			cmbShikigami.SelectedIndex = -1;
		}

		private void clearMitamaInputs()
		{
			initializeMainStatComboBoxes();
			initializeSubStatComboBoxes();

			clearMainValueTextBoxes();
			clearSubValueTextBoxes();
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
			initializeSetEffectComboBoxes();
			initializeUniqueEffectComboBoxes();
		}

		private void clearResultTexts()
		{
			txtMitamaOnly.Text = "";
			txtFinalStats.Text = "";
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
			applyShikigamiFilter();
		}

		private void applyShikigamiFilter()
		{
			var selectedShikigami = cmbShikigami.SelectedItem as ShikigamiDto;
			string selectedShikigamiName = selectedShikigami?.Name;

			string selectedRarity = cmbRarityFilter.Text;

			var filteredList = mShikigamiList;

			if (selectedRarity != DisplayText.RARITY_ALL)
			{
				filteredList = mShikigamiList.Where(x => x.Rarity == selectedRarity).ToList();
			}

			cmbShikigami.DataSource = null;
			cmbShikigami.DataSource = filteredList;
			cmbShikigami.DisplayMember = "Name";
			cmbShikigami.SelectedIndex = -1;

			if (!string.IsNullOrEmpty(selectedShikigamiName))
			{
				trySelectShikigami(selectedShikigamiName);
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
				ShikigamiDataOutcomeDto outcome = ShikigamiGateway.GetRecoveryCandinateShikigamiList(AppPath.ShikigamiDataCsvPath, dialog.FileName, out recoveryShikigamiList);

				if (ShikigamiDataErrorHandler.Handle(outcome, "式神復旧候補読み込み"))
				{
					return;
				}

				if (recoveryShikigamiList.Count == 0)
				{
					String message = "復旧できる式神はありませんでした。";
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

					ShikigamiDataFileManager.CreateBackup();

					outcome = ShikigamiGateway.RecoveryShikigami(AppPath.ShikigamiDataCsvPath, shikigamiRecoveryDialog.SelectedRecoveryCandidates);

					if (ShikigamiDataErrorHandler.Handle(outcome, "式神復旧"))
					{
						return;
					}

					String message = "式神の復旧に成功しました。";
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
	}
}
