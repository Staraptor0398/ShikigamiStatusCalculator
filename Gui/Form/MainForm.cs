using Gui.Common;
using Gui.Converter;
using Gui.Dialog;
using Gui.Factory;
using Gui.Form.Control;
using Gui.IO;
using Gui.SaveData;
using Gui.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Gui.Form
{
	public partial class MainForm : System.Windows.Forms.Form
	{
		/****************************************************************************************************
		  UI入力コントロール取得
		****************************************************************************************************/
		private MitamaSlotInputControl[] getMitamaSlotInputControls()
		{
			return new MitamaSlotInputControl[]
			{
				new MitamaSlotInputControl
				{
					MainStatComboBox = cmbMainStat1,
					MainValueTextBox = txtMainVal1,
					SubStats=new SubStatInputControl[]
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

		private ComboBox[] getSetEffectComboBoxes()
		{
			return new ComboBox[]
			{
				cmbSetBonus1,
				cmbSetBonus2,
				cmbSetBonus3
			};
		}

		private ComboBox[] getUniqueEffectComboBoxes()
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
		private CalculationResultDto _lastCalculationResult = null;

		private bool _isCalculationResultDirty = true;

		private List<ShikigamiDto> _shikigamiList = null;
		/****************************************************************************************************
		  コンストラクタ
		****************************************************************************************************/
		public MainForm()
		{
			InitializeComponent();

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

			if (selected == null)
			{
				txtBaseStats.Text = "";
			}
			else
			{
				txtBaseStats.Text =
						$"{DisplayText.Attack}: {selected.Attack:F2} " +
						$"{DisplayText.HP}: {selected.HP:F2} " +
						$"{DisplayText.Defense}: {selected.Defense:F2} " +
						$"{DisplayText.Speed}: {selected.Speed:F2} " +
						$"{DisplayText.CriticalRate}: {selected.CriticalRate:F2}% " +
						$"{DisplayText.CriticalDamage}: {selected.CriticalDamage:F2}% " +
						$"{DisplayText.EffectHit}: {selected.EffectHit:F2}% " +
						$"{DisplayText.EffectResist}: {selected.EffectResist:F2}%";

			}

			markCalculationResultDirty();
		}

		/****************************************************************************************************
		  ステータス計算
		****************************************************************************************************/
		private void btnCalc_Click(object sender, EventArgs e)
		{
			CalculationInputValidationOutcome validationOutcome = validateCalculationInput();

			if (CalculationInputValidationErrorHandler.Handle(validationOutcome))
			{
				return;
			}

			var baseStatus = getSelectedShikigamiStatus();
			var mitamaSet = MitamaSetFactory.Create(getMitamaSlotInputControls(), getSetEffectComboBoxes(), getUniqueEffectComboBoxes());

			try
			{
				_lastCalculationResult = CalculationGateway.Calclutate(baseStatus, mitamaSet);
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

			showCalculationResult(_lastCalculationResult);
		}

		private void markCalculationResultClean()
		{
			_isCalculationResultDirty = false;
		}

		private void markCalculationResultDirty()
		{
			updateSaveButtonEnabled();

			if (_lastCalculationResult == null)
			{
				return;
			}

			if (_isCalculationResultDirty)
			{
				return;
			}

			_isCalculationResultDirty = true;

			Logger.Info("Operation=ステータス計算結果状態変更 Message=入力内容が変更されたため、前回の計算結果を無効化しました。");
		}

		private StatusDto getSelectedShikigamiStatus()
		{
			var selected = (ShikigamiDto)cmbShikigami.SelectedItem;

			if (selected == null)
			{
				return new StatusDto();
			}

			return new StatusDto
			{
				Attack = selected.Attack,
				HP = selected.HP,
				Defense = selected.Defense,
				Speed = selected.Speed,

				CritRate = selected.CriticalRate,
				CritDamage = selected.CriticalDamage,
				EffectHit = selected.EffectHit,
				EffectResist = selected.EffectResist,

				AdditionalAttackRate = 0.0,
				AdditionalHpRate = 0.0,
				AdditionalDefenseRate = 0.0,
			};
		}

		/****************************************************************************************************
		  計算入力検証
		****************************************************************************************************/
		private CalculationInputValidationOutcome validateCalculationInput()
		{
			CalculationInputValidationOutcome outcome;

			outcome = validateEqueppedMitamaCount();

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateSubStatsInUnequippedSlots();

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateEffectSlotCount();

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			outcome = validateSubStats();

			if (outcome != CalculationInputValidationOutcome.SUCCESS)
			{
				return outcome;
			}

			return outcome;
		}

		private CalculationInputValidationOutcome validateEqueppedMitamaCount()
		{
			if (getEquippedSlotCount() <= 0)
			{
				return CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private CalculationInputValidationOutcome validateSubStatsInUnequippedSlots()
		{
			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				CalculationInputValidationOutcome outcome = validateSubStatsInUnequippedSlot(slot);

				if (outcome != CalculationInputValidationOutcome.SUCCESS)
				{
					return outcome;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private CalculationInputValidationOutcome validateSubStatsInUnequippedSlot(MitamaSlotInputControl slot)
		{
			if (!string.IsNullOrWhiteSpace(slot.MainStatComboBox.Text))
			{
				return CalculationInputValidationOutcome.SUCCESS;
			}

			foreach (SubStatInputControl subsStat in slot.SubStats)
			{
				if (hasSubStatInput(subsStat.TypeComboBox, subsStat.ValueTextBox))
				{
					return CalculationInputValidationOutcome.MAIN_STAT_NOT_SELECTED_WITH_SUB_STAT;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private bool hasSubStatInput(ComboBox cmbSubStat, TextBox txtSubvalue)
		{
			return (!string.IsNullOrWhiteSpace(cmbSubStat.Text) && cmbSubStat.Text != DisplayText.None) || !string.IsNullOrWhiteSpace(txtSubvalue.Text);
		}

		private CalculationInputValidationOutcome validateEffectSlotCount()
		{
			int equippedSlotCount = getEquippedSlotCount();

			int setEffectCount = getSelectedSetEffectCount();
			int uniqueEffectCount = getSelectedUniqueEffectCount();

			int usedSlotCount = setEffectCount * 2 + uniqueEffectCount;

			if (usedSlotCount > equippedSlotCount)
			{
				return CalculationInputValidationOutcome.EFFECT_SLOT_COUNT_EXCEEDS_EQUIPPED_SLOTS;
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private int getEquippedSlotCount()
		{
			int count = 0;

			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				if (!string.IsNullOrWhiteSpace(slot.MainStatComboBox.Text))
				{
					count++;
				}
			}

			return count;
		}

		private int getSelectedSetEffectCount()
		{
			int count = 0;

			foreach (ComboBox comboBox in getSetEffectComboBoxes())
			{
				if (isSelectedEffect(comboBox))
				{
					count++;
				}
			}

			return count;
		}

		private int getSelectedUniqueEffectCount()
		{
			int count = 0;

			foreach (ComboBox comboBox in getUniqueEffectComboBoxes())
			{
				if (isSelectedEffect(comboBox))
				{
					count++;
				}
			}

			return count;
		}

		private bool isSelectedEffect(ComboBox comboBox)
		{
			if (string.IsNullOrWhiteSpace(comboBox.Text))
			{
				return false;
			}

			if (comboBox.Text == DisplayText.None)
			{
				return false;
			}

			return true;
		}

		private CalculationInputValidationOutcome validateSubStats()
		{
			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				CalculationInputValidationOutcome outcome = validateSubStatsInSlot(slot.SubStats);

				if (outcome != CalculationInputValidationOutcome.SUCCESS)
				{
					return outcome;
				}
			}

			return CalculationInputValidationOutcome.SUCCESS;
		}

		private CalculationInputValidationOutcome validateSubStatsInSlot(SubStatInputControl[] subStats)
		{
			List<string> selectedSubStats = new List<string>();

			foreach (SubStatInputControl subStat in subStats)
			{
				bool hasType = !string.IsNullOrWhiteSpace(subStat.TypeComboBox.Text) && subStat.TypeComboBox.Text != DisplayText.None;
				bool hasValue = !string.IsNullOrWhiteSpace(subStat.ValueTextBox.Text);

				if (!hasType && !hasValue)
				{
					return CalculationInputValidationOutcome.SUCCESS;
				}

				if (hasType && !hasValue)
				{
					return CalculationInputValidationOutcome.SUB_STAT_TYPE_WHITHOUT_VALUE;
				}

				if (!hasType && hasValue)
				{
					return CalculationInputValidationOutcome.SUB_STAT_VALUE_WHITHOUT_TYPE;
				}

				if (!double.TryParse(subStat.ValueTextBox.Text, out double value))
				{
					return CalculationInputValidationOutcome.INVALID_VALUE;
				}

				if (value < 0)
				{
					return CalculationInputValidationOutcome.NEGATIVE_VALUE;
				}

				if (selectedSubStats.Contains(subStat.TypeComboBox.Text))
				{
					return CalculationInputValidationOutcome.DUPLICATE_SUB_STAT;
				}

				selectedSubStats.Add(subStat.TypeComboBox.Text);
			}

			return CalculationInputValidationOutcome.SUCCESS;
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

			txtMitamaOnly.Text = formatMitamaStatus(result.MitamaOnlyStatus);

			if (cmbShikigami.SelectedItem != null)
			{
				txtFinalStats.Text = formatFinalStatus(result.FinalStatus);
			}
			else
			{
				txtFinalStats.Text = "";
			}
		}

		private string formatMitamaStatus(StatusDto s)
		{
			if (s == null)
			{
				return "";
			}

			return
					$"{DisplayText.Attack}: {s.Attack:F2} " +
					$"{DisplayText.HP}: {s.HP:F2} " +
					$"{DisplayText.Defense}: {s.Defense:F2} " +
					$"{DisplayText.Speed}: {s.Speed:F2} " +

					$"{DisplayText.AdditionalAttackRate}: {s.AdditionalAttackRate:F2}% " +
					$"{DisplayText.AdditionalHPRate}: {s.AdditionalHpRate:F2}% " +
					$"{DisplayText.AdditionalDefenseRate}: {s.AdditionalDefenseRate:F2}% " +

					$"{DisplayText.CriticalRate}: {s.CritRate:F2}% " +
					$"{DisplayText.CriticalDamage}: {s.CritDamage:F2}% " +
					$"{DisplayText.EffectHit}: {s.EffectHit:F2}% " +
					$"{DisplayText.EffectResist}: {s.EffectResist:F2}%";
		}

		private string formatFinalStatus(StatusDto s)
		{
			if (s == null)
			{
				return "";
			}

			return
					$"{DisplayText.Attack}: {s.Attack:F2} " +
					$"{DisplayText.HP}: {s.HP:F2} " +
					$"{DisplayText.Defense}: {s.Defense:F2} " +
					$"{DisplayText.Speed}: {s.Speed:F2} " +
					$"{DisplayText.CriticalRate}: {s.CritRate:F2}% " +
					$"{DisplayText.CriticalDamage}: {s.CritDamage:F2}% " +
					$"{DisplayText.EffectHit}: {s.EffectHit:F2}% " +
					$"{DisplayText.EffectResist}: {s.EffectResist:F2}%";
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
			initializeComboBoxes();
			registerCalculationInputChangedEvents();
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
				DisplayText.RarityAll,
				DisplayText.RarityUR,
				DisplayText.RaritySP,
				DisplayText.RaritySSR,
				DisplayText.RaritySR);

			cmbRarityFilter.SelectedIndex = 0;
		}

		private void initializeShikigamiComboBox()
		{
			var outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out _shikigamiList);

			if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ読み込み"))
			{
				ShikigamiDataFileManager.MoveBrokenFile();
				ShikigamiDataFileManager.RestoreDefaultIfMissing();

				outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out _shikigamiList);

				if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ復元後読み込み"))
				{
					_shikigamiList = new List<ShikigamiDto>();

				}
			}

			cmbShikigami.DataSource = _shikigamiList;
			cmbShikigami.DisplayMember = "Name";

			cmbShikigami.SelectedIndex = -1;
			txtBaseStats.Text = "";
		}

		private void initializeMainStatComboBoxes()
		{
			setComboItems(cmbMainStat1,
				DisplayText.Attack);

			setComboItems(cmbMainStat2,
				DisplayText.Speed,
				DisplayText.AdditionalAttackRate,
				DisplayText.AdditionalHPRate,
				DisplayText.AdditionalDefenseRate);

			setComboItems(cmbMainStat3,
				DisplayText.Defense);

			setComboItems(cmbMainStat4,
				DisplayText.EffectHit,
				DisplayText.EffectResist,
				DisplayText.AdditionalAttackRate,
				DisplayText.AdditionalHPRate,
				DisplayText.AdditionalDefenseRate);

			setComboItems(cmbMainStat5,
				DisplayText.HP);

			setComboItems(cmbMainStat6,
				DisplayText.CriticalRate,
				DisplayText.CriticalDamage,
				DisplayText.AdditionalAttackRate,
				DisplayText.AdditionalHPRate,
				DisplayText.AdditionalDefenseRate);
		}

		private void initializeSubStatComboBoxes()
		{
			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					setComboItems(subStat.TypeComboBox,
						DisplayText.None,
						DisplayText.Speed,
						DisplayText.AdditionalAttackRate,
						DisplayText.AdditionalHPRate,
						DisplayText.AdditionalDefenseRate,
						DisplayText.CriticalRate,
						DisplayText.CriticalDamage,
						DisplayText.EffectHit,
						DisplayText.EffectResist,
						DisplayText.Attack,
						DisplayText.HP,
						DisplayText.Defense);
				}
			}
		}

		private void initializeSetEffectComboBoxes()
		{
			foreach (ComboBox comboBox in getSetEffectComboBoxes())
			{
				setComboItems(comboBox,
					DisplayText.None,
					DisplayText.CriticalRate,
					DisplayText.CriticalDamage,
					DisplayText.EffectHit,
					DisplayText.EffectResist,
					DisplayText.AdditionalAttackRate,
					DisplayText.AdditionalHPRate,
					DisplayText.AdditionalDefenseRate);
			}
		}

		private void initializeUniqueEffectComboBoxes()
		{
			foreach (ComboBox comboBox in getUniqueEffectComboBoxes())
			{
				setComboItems(comboBox,
					DisplayText.None,
					DisplayText.CriticalRate,
					DisplayText.EffectHit,
					DisplayText.EffectResist,
					DisplayText.AdditionalAttackRate,
					DisplayText.AdditionalHPRate,
					DisplayText.AdditionalDefenseRate);
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
			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					subStat.TypeComboBox.SelectedIndexChanged += calculationInputChanged;
					subStat.ValueTextBox.TextChanged += calculationInputChanged;
				}
			}

			foreach (ComboBox comboBox in getSetEffectComboBoxes())
			{
				comboBox.SelectedIndexChanged += calculationInputChanged;
			}

			foreach (ComboBox comboBox in getUniqueEffectComboBoxes())
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
			var value = getMainStatValue(cmbMainStat1.SelectedItem.ToString(), 1);
			txtMainVal1.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat2_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat2.SelectedItem.ToString(), 2);
			txtMainVal2.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat3_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat3.SelectedItem.ToString(), 3);
			txtMainVal3.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat4_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat4.SelectedItem.ToString(), 4);
			txtMainVal4.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat5_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat5.SelectedItem.ToString(), 5);
			txtMainVal5.Text = value.ToString();

			markCalculationResultDirty();
		}

		private void cmbMainStat6_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat6.SelectedItem.ToString(), 6);
			txtMainVal6.Text = value.ToString();

			markCalculationResultDirty();
		}

		private double getMainStatValue(string text, int slot)
		{
			double ret = 0.0;

			switch (slot)
			{
				case 1:
					ret = 486.0;
					break;
				case 2:
					if (text == DisplayText.Speed)
					{
						ret = 57.0;
					}
					else
					{
						ret = 55.0;
					}
					break;
				case 3:
					ret = 104.0;
					break;
				case 4:
					ret = 55.0;
					break;
				case 5:
					ret = 2052.0;
					break;
				case 6:
					if (text == DisplayText.CriticalDamage)
					{
						ret = 89.0;
					}
					else
					{
						ret = 55.0;
					}
					break;
				default:
					break;
			}

			return ret;
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
					if (dialog._selectedSaveType == SaveDataSaveType.Build)
					{
						var data = BuildSaveDataFactory.Create(cmbShikigami, getMitamaSlotInputControls(), getSetEffectComboBoxes(), getUniqueEffectComboBoxes());
						SaveDataAccess.SaveBuild(dialog._filePath, data);
					}
					else if (dialog._selectedSaveType == SaveDataSaveType.MitamaSet)
					{
						var data = MitamaSetSaveDataFactory.Create(getMitamaSlotInputControls(), getSetEffectComboBoxes(), getUniqueEffectComboBoxes());
						SaveDataAccess.SaveMitamaSet(dialog._filePath, data);
					}
					else if (dialog._selectedSaveType == SaveDataSaveType.CalculationSnapshot)
					{
						string snapshotName = createSnapshotNameFromFilePath(dialog._filePath);

						var data = CalculationSnapshotSaveDataFactory.Create(cmbShikigami, getMitamaSlotInputControls(), getSetEffectComboBoxes(), getUniqueEffectComboBoxes(), snapshotName, _lastCalculationResult);
						SaveDataAccess.SaveSnapshot(dialog._filePath, data);
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
			btnSave.Enabled = validateCalculationInput() != CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA;
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
					if (dialog._selectedLoadType == SaveDataLoadType.Build)
					{
						var data = SaveDataAccess.LoadBuild(dialog._filePath);
						applyBuildSaveDataToUI(data);
					}
					else if (dialog._selectedLoadType == SaveDataLoadType.MitamaSet)
					{
						var data = SaveDataAccess.LoadMitamaSet(dialog._filePath);
						applyMitamaSetSaveDataToUI(data);
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
			applyMitamaSetSaveDataToUI(data.MitamaSet);
		}

		private void applyMitamaSetSaveDataToUI(MitamaSetSaveData data)
		{
			if (data == null)
			{
				return;
			}

			applyMitama(data.Mitamas);
			applySetEffect(data.SetEffects);
			applyUniqueEffect(data.UniqueEffects);
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

			cmbRarityFilter.SelectedItem = DisplayText.RarityAll;

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

		private void applyMitama(List<MitamaSaveData> list)
		{
			if (list == null)
			{
				return;
			}

			MitamaSlotInputControl[] slots = getMitamaSlotInputControls();

			for (int i = 0; i < list.Count && i < slots.Length; i++)
			{
				applySingleMitama(list[i], slots[i]);
			}
		}

		private void applySingleMitama(MitamaSaveData data, MitamaSlotInputControl slot)
		{
			if (data == null || slot == null)
			{
				return;
			}

			slot.MainStatComboBox.Text = data.MainStat.Type;
			slot.MainValueTextBox.Text = data.MainStat.Value.ToString();

			for (int i = 0; i < data.SubStats.Count && i < slot.SubStats.Length; i++)
			{
				applyEffect(data.SubStats[i], slot.SubStats[i].TypeComboBox, slot.SubStats[i].ValueTextBox);
			}
		}

		private void applySetEffect(List<EffectSaveData> list)
		{
			if (list == null)
			{
				return;
			}

			ComboBox[] comboBoxes = getSetEffectComboBoxes();

			for (int i = 0; i < list.Count && i < comboBoxes.Length; i++)
			{
				applyEffect(list[i], comboBoxes[i]);
			}
		}

		private void applyUniqueEffect(List<EffectSaveData> list)
		{
			if (list == null)
			{
				return;
			}

			ComboBox[] comboBoxes = getUniqueEffectComboBoxes();

			for (int i = 0; i < list.Count && i < comboBoxes.Length; i++)
			{
				applyEffect(list[i], comboBoxes[i]);
			}
		}

		private void applyEffect(EffectSaveData data, ComboBox comboBox, TextBox textBox)
		{
			if (data == null || comboBox == null || textBox == null)
			{
				return;
			}

			comboBox.SelectedItem = data.Type;
			textBox.Text = data.Value.ToString();
		}

		private void applyEffect(EffectSaveData data, ComboBox comboBox)
		{
			if (data == null || comboBox == null)
			{
				return;
			}

			comboBox.SelectedItem = data.Type;
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
			return _lastCalculationResult != null && !_isCalculationResultDirty;
		}

		private string createSnapshotNameFromFilePath(string filePath)
		{
			string fileName = Path.GetFileName(filePath);

			if (fileName.EndsWith(SaveDataFileDefinition.SnapshotExtension))
			{
				return fileName.Substring(0, fileName.Length - SaveDataFileDefinition.SnapshotExtension.Length);
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

					baseSnapshot = SaveDataAccess.LoadSnapshot(dialog._baseSnapshotFilePath);
					targetSnapshot = SaveDataAccess.LoadSnapshot(dialog._targetSnapshotFilePath);

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

					StatusDto baseStatus = StatusSaveDataConverter.ToDto(baseSnapshot.FinalStatus);
					StatusDto targetStatus = StatusSaveDataConverter.ToDto(targetSnapshot.FinalStatus);

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

			using (var form = new ShikigamiResisterForm())
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

			using (var form = new ShikigamiResisterForm(selectedShikigami))
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
			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				slot.MainValueTextBox.Text = "";
			}
		}

		private void clearSubValueTextBoxes()
		{
			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
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
			if (_lastCalculationResult == null)
			{
				MessageBox.Show(
					"先に計算を実行してください。",
					"確認",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			using (var form = new ResultViewForm(_lastCalculationResult))
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

			var filteredList = _shikigamiList;

			if (selectedRarity != DisplayText.RarityAll)
			{
				filteredList = _shikigamiList.Where(x => x.Rarity == selectedRarity).ToList();
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

					outcome = ShikigamiGateway.RecoveryShikigami(AppPath.ShikigamiDataCsvPath, shikigamiRecoveryDialog._selectedRecoveryCandinate);

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
