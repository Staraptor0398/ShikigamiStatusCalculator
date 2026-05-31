using Gui.Common;
using Gui.Dialog;
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
		  UI入力コントロール定義
		****************************************************************************************************/
		private class SubStatInputControl
		{
			public ComboBox TypeComboBox { get; set; }
			public TextBox ValueTextBox { get; set; }
		}

		private class MitamaSlotInputControl
		{
			public ComboBox MainStatComboBox { get; set; }
			public TextBox MainValueTextBox { get; set; }
			public SubStatInputControl[] SubStats { get; set; }
		}

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
			var mitamaSet = createMitamaSetDto();

			_lastCalculationResult = CalculationGateway.Calclutate(baseStatus, mitamaSet);

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
		  Dto作成
		****************************************************************************************************/
		private MitamaSetDto createMitamaSetDto()
		{
			MitamaSetDto dto = new MitamaSetDto();

			dto.Mitamas = new List<MitamaDto>();

			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				dto.Mitamas.Add(createMitamaDto(slot));
			}

			dto.SetEffects = createSetEffectDtos();
			dto.UniqueEffects = createUniqueEffectDtos();

			return dto;
		}

		private MitamaDto createMitamaDto(MitamaSlotInputControl slot)
		{
			if (slot == null)
			{
				return null;
			}

			return new MitamaDto
			{
				MainStat = createStatValueDto(slot.MainStatComboBox, slot.MainValueTextBox),
				SubStat = createSubStatValueDtos(slot.SubStats)
			};
		}

		private List<StatValueDto> createSubStatValueDtos(SubStatInputControl[] subStats)
		{
			if (subStats == null)
			{
				return null;
			}

			List<StatValueDto> list = new List<StatValueDto>();

			foreach (SubStatInputControl subStat in subStats)
			{
				list.Add(createStatValueDto(subStat.TypeComboBox, subStat.ValueTextBox));
			}

			return list;
		}

		private StatValueDto createStatValueDto(ComboBox cmbType, TextBox txtValue)
		{
			if (cmbType == null || txtValue == null)
			{
				return null;
			}

			var dto = new StatValueDto();

			dto.Type = convertStatType(cmbType.Text);

			if (double.TryParse(txtValue.Text, out double value))
			{
				dto.Value = value;
			}
			else
			{
				dto.Value = 0.0;
			}

			return dto;
		}

		private List<SetEffectDto> createSetEffectDtos()
		{
			List<SetEffectDto> list = new List<SetEffectDto>();

			foreach (ComboBox comboBox in getSetEffectComboBoxes())
			{
				list.Add(createSetEffectDto(comboBox));
			}
			return list;
		}

		private SetEffectDto createSetEffectDto(ComboBox cmbEffect)
		{
			if (cmbEffect == null)
			{
				return null;
			}

			return new SetEffectDto
			{
				Stat = new StatValueDto
				{
					Type = convertStatType(cmbEffect.Text),
					Value = getSetEffectValue(cmbEffect.Text)
				}
			};
		}

		private List<SetEffectDto> createUniqueEffectDtos()
		{
			List<SetEffectDto> list = new List<SetEffectDto>();

			foreach (ComboBox comboBox in getUniqueEffectComboBoxes())
			{
				list.Add(createUniqueEffectDto(comboBox));
			}

			return list;
		}

		private SetEffectDto createUniqueEffectDto(ComboBox cmbEffect)
		{
			if (cmbEffect == null)
			{
				return null;
			}

			return new SetEffectDto
			{
				Stat = new StatValueDto
				{
					Type = convertStatType(cmbEffect.Text),
					Value = getUniqueEffectValue(cmbEffect.Text)
				}
			};
		}

		private StatTypeDto convertStatType(string text)
		{
			StatTypeDto ret = StatTypeDto.None;

			switch (text)
			{
				case DisplayText.Attack: ret = StatTypeDto.Attack; break;
				case DisplayText.HP: ret = StatTypeDto.Hp; break;
				case DisplayText.Defense: ret = StatTypeDto.Defense; break;
				case DisplayText.Speed: ret = StatTypeDto.Speed; break;
				case DisplayText.CriticalRate: ret = StatTypeDto.CriticalRate; break;
				case DisplayText.CriticalDamage: ret = StatTypeDto.CriticalDamage; break;
				case DisplayText.EffectHit: ret = StatTypeDto.EffectHit; break;
				case DisplayText.EffectResist: ret = StatTypeDto.EffectResist; break;
				case DisplayText.AdditionalAttackRate: ret = StatTypeDto.AdditionalAttackRate; break;
				case DisplayText.AdditionalHPRate: ret = StatTypeDto.AdditionalHpRate; break;
				case DisplayText.AdditionalDefenseRate: ret = StatTypeDto.AdditionalDefenseRate; break;
				default: break;
			}

			return ret;
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

		private double getSetEffectValue(string text)
		{
			double ret = 0.0;

			switch (text)
			{
				case DisplayText.AdditionalAttackRate:
				case DisplayText.AdditionalHPRate:
				case DisplayText.CriticalRate:
				case DisplayText.EffectHit:
				case DisplayText.EffectResist:
					ret = 15.0;
					break;
				case DisplayText.CriticalDamage:
					ret = 20.0;
					break;
				case DisplayText.AdditionalDefenseRate:
					ret = 30.0;
					break;
				default:
					break;
			}

			return ret;
		}

		private double getUniqueEffectValue(string text)
		{
			double ret = 0.0;

			switch (text)
			{
				case DisplayText.AdditionalAttackRate:
				case DisplayText.AdditionalHPRate:
				case DisplayText.CriticalRate:
				case DisplayText.EffectHit:
				case DisplayText.EffectResist:
					ret = 8.0;
					break;
				case DisplayText.AdditionalDefenseRate:
					ret = 16.0;
					break;
				default:
					break;
			}

			return ret;
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
				_shikigamiList = new List<ShikigamiDto>();
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

		/****************************************************************************************************
		  SaveData保存
		****************************************************************************************************/
		private void btnSave_Click(object sender, EventArgs e)
		{
			using (var dialog = new SaveDataSaveDialog(cmbShikigami.Text, getSaveDataSaveLevel()))
			{
				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				setSaveDataOperationButtonsEnabled(false);

				try
				{
					if (dialog._selectedSaveType == SaveDataSaveType.Build)
					{
						var data = createCurrentBuildSaveData();
						SaveDataAccess.SaveBuild(dialog._filePath, data);
					}
					else if (dialog._selectedSaveType == SaveDataSaveType.MitamaSet)
					{
						var data = createCurrentMitamaSetSaveData();
						SaveDataAccess.SaveMitamaSet(dialog._filePath, data);
					}
					else if (dialog._selectedSaveType == SaveDataSaveType.CalculationSnapshot)
					{
						string snapshotName = createSnapshotNameFromFilePath(dialog._filePath);

						var data = createCurrentCalculationSnapshotSaveData(snapshotName);
						SaveDataAccess.SaveSnapshot(dialog._filePath, data);
					}
				}
				finally
				{
					setSaveDataOperationButtonsEnabled(true);
				}
			}
		}

		private BuildSaveData createCurrentBuildSaveData()
		{
			return new BuildSaveData
			{
				ShikigamiName = cmbShikigami.Text,
				MitamaSet = createCurrentMitamaSetSaveData()
			};
		}

		private MitamaSetSaveData createCurrentMitamaSetSaveData()
		{
			return new MitamaSetSaveData
			{
				Mitamas = createMitamaSaveDataList(),
				SetEffects = createSetEffectSaveData(),
				UniqueEffects = createUniqueEffectSaveData()
			};
		}

		private List<MitamaSaveData> createMitamaSaveDataList()
		{
			List<MitamaSaveData> list = new List<MitamaSaveData>();

			MitamaSlotInputControl[] slots = getMitamaSlotInputControls();

			for (int i = 0; i < slots.Length; i++)
			{
				list.Add(createMitamaSaveData(i + 1, slots[i]));
			}

			return list;
		}

		private MitamaSaveData createMitamaSaveData(int slot, MitamaSlotInputControl input)
		{
			if (0 > slot || slot > 6 || input == null)
			{
				return null;
			}

			return new MitamaSaveData
			{
				Slot = slot,

				MainStat = new EffectSaveData
				{
					Type = input.MainStatComboBox.Text,
					Value = getMainStatValue(input.MainValueTextBox.Text, slot)
				},

				SubStats = createSubStatSaveDataList(input.SubStats)
			};
		}

		private List<EffectSaveData> createSubStatSaveDataList(SubStatInputControl[] subStats)
		{
			if (subStats == null)
			{
				return null;
			}

			List<EffectSaveData> list = new List<EffectSaveData>();

			foreach (SubStatInputControl subStat in subStats)
			{
				list.Add(createEffectSaveData(subStat.TypeComboBox, subStat.ValueTextBox));
			}

			return list;
		}

		private List<EffectSaveData> createSetEffectSaveData()
		{
			List<EffectSaveData> list = new List<EffectSaveData>();

			foreach (ComboBox comboBox in getSetEffectComboBoxes())
			{
				list.Add(createEffectSaveData(comboBox));
			}

			return list;
		}

		private List<EffectSaveData> createUniqueEffectSaveData()
		{
			List<EffectSaveData> list = new List<EffectSaveData>();

			foreach (ComboBox comboBox in getUniqueEffectComboBoxes())
			{
				list.Add(createEffectSaveData(comboBox));
			}

			return list;
		}

		private EffectSaveData createEffectSaveData(ComboBox cmb, TextBox txt)
		{
			if (cmb == null || txt == null)
			{
				return null;
			}

			double.TryParse(txt.Text, out double value);

			return new EffectSaveData
			{
				Type = cmb.Text,
				Value = value
			};
		}

		private EffectSaveData createEffectSaveData(ComboBox cmb)
		{
			if (cmb == null)
			{
				return null;
			}

			return new EffectSaveData
			{
				Type = cmb.Text,
				Value = 0.0
			};
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
				if (dialog.ShowDialog() != DialogResult.OK)
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

		private CalculationSnapshotSaveData createCurrentCalculationSnapshotSaveData(string snapshotName)
		{
			if (string.IsNullOrEmpty(snapshotName))
			{
				return null;
			}

			return new CalculationSnapshotSaveData
			{
				SnapshotName = snapshotName,
				CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
				ShikigamiName = cmbShikigami.Text,
				MitamaSet = createCurrentMitamaSetSaveData(),
				MitamaStatus = createStatusSaveData(_lastCalculationResult.MitamaOnlyStatus),
				FinalStatus = createStatusSaveData(_lastCalculationResult.FinalStatus)
			};
		}

		private StatusSaveData createStatusSaveData(StatusDto status)
		{
			if (status == null)
			{
				return null;
			}

			return new StatusSaveData
			{
				Attack = status.Attack,
				HP = status.HP,
				Deffense = status.Defense,
				Speed = status.Speed,
				CritRate = status.CritRate,
				CritDamage = status.CritDamage,
				EffectHit = status.EffectHit,
				EffectResist = status.EffectResist
			};
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

				CalculationSnapshotSaveData baseSnapshot = SaveDataAccess.LoadSnapshot(dialog._baseSnapshotFilePath);
				CalculationSnapshotSaveData targetSnapshot = SaveDataAccess.LoadSnapshot(dialog._targetSnapshotFilePath);

				StatusDto baseStatus = createStatusDto(baseSnapshot.FinalStatus);
				StatusDto targetStatus = createStatusDto(targetSnapshot.FinalStatus);

				if (baseStatus == null || targetStatus == null)
				{
					return;
				}

				StatusComparisonResultDto comparisonResult = ComparisonGateway.CompareStatus(baseStatus, targetStatus);

				string baseSnapshotName = baseSnapshot.SnapshotName;
				string targetSnapshotName = targetSnapshot.SnapshotName;

				using (var form = new StatusComparisonResultForm(baseSnapshotName, targetSnapshotName, comparisonResult))
				{
					form.ShowDialog(this);
				}
			}
		}

		private StatusDto createStatusDto(StatusSaveData saveData)
		{
			if (saveData == null)
			{
				return null;
			}

			return new StatusDto
			{
				Attack = saveData.Attack,
				HP = saveData.HP,
				Defense = saveData.Deffense,
				Speed = saveData.Speed,
				CritRate = saveData.CritRate,
				CritDamage = saveData.CritDamage,
				EffectHit = saveData.EffectHit,
				EffectResist = saveData.EffectResist
			};
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
	}
}
