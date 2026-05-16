using Gui.Common;
using Gui.IO;
using Gui.SaveData;
using Gui.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ShikigamiApp
{
	public partial class MainForm : Form
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
				return;
			}

			txtBaseStats.Text =
					$"{DisplayText.Attack}: {selected.Attack:F2} " +
					$"{DisplayText.HP}: {selected.HP:F2} " +
					$"{DisplayText.Deffense}: {selected.Defense:F2} " +
					$"{DisplayText.Speed}: {selected.Speed:F2} " +
					$"{DisplayText.CritRate}: {selected.CritRate:F2}% " +
					$"{DisplayText.CritDamage}: {selected.CritDamage:F2}% " +
					$"{DisplayText.EffectHit}: {selected.EffectHit:F2}% " +
					$"{DisplayText.EffectResist}: {selected.EffectResist:F2}%";

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
			var mitamaSet = buildMitamaSetDto();

			_lastCalculationResult = CalculationGateway.calclutate(baseStatus, mitamaSet);
			markCalculationResultClean();

			showCalculationResult(_lastCalculationResult);
		}

		private void markCalculationResultClean()
		{
			_isCalculationResultDirty = false;
		}

		private void markCalculationResultDirty()
		{
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

				CritRate = selected.CritRate,
				CritDamage = selected.CritDamage,
				EffectHit = selected.EffectHit,
				EffectResist = selected.EffectResist,

				AdditionalAttackRate = 0.0,
				AdditionalHpRate = 0.0,
				AdditionalDeffenseRate = 0.0,
			};
		}

		/****************************************************************************************************
		  計算入力検証
		****************************************************************************************************/
		private CalculationInputValidationOutcome validateCalculationInput()
		{
			CalculationInputValidationOutcome outcome;

			outcome = validateSelectedShikigami();

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

		private CalculationInputValidationOutcome validateSelectedShikigami()
		{
			if (cmbShikigami.SelectedItem == null)
			{
				return CalculationInputValidationOutcome.SHIKIGAMI_NOT_SELECTED;
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

		private CalculationInputValidationOutcome validateSubStatsInSlot(
			SubStatInputControl[] subStats)
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
		private MitamaSetDto buildMitamaSetDto()
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
			return new MitamaDto
			{
				MainStat = createStatValueDto(slot.MainStatComboBox, slot.MainValueTextBox),
				SubStat = createSubStatValueDtos(slot.SubStats)
			};
		}

		private List<StatValueDto> createSubStatValueDtos(SubStatInputControl[] subStats)
		{
			List<StatValueDto> list = new List<StatValueDto>();

			foreach (SubStatInputControl subStat in subStats)
			{
				list.Add(createStatValueDto(subStat.TypeComboBox, subStat.ValueTextBox));
			}

			return list;
		}

		private StatValueDto createStatValueDto(ComboBox cmbType, TextBox txtValue)
		{
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

		private List<StatusEffectDto> createSetEffectDtos()
		{
			List<StatusEffectDto> list = new List<StatusEffectDto>();

			foreach (ComboBox comboBox in getSetEffectComboBoxes())
			{
				list.Add(createSetEffectDto(comboBox));
			}
			return list;
		}

		private StatusEffectDto createSetEffectDto(ComboBox cmbEffect)
		{
			return new StatusEffectDto
			{
				Stat = new StatValueDto
				{
					Type = convertStatType(cmbEffect.Text),
					Value = getSetEffectValue(cmbEffect.Text)
				}
			};
		}

		private List<StatusEffectDto> createUniqueEffectDtos()
		{
			List<StatusEffectDto> list = new List<StatusEffectDto>();

			foreach (ComboBox comboBox in getUniqueEffectComboBoxes())
			{
				list.Add(createUniqueEffectDto(comboBox));
			}

			return list;
		}

		private StatusEffectDto createUniqueEffectDto(ComboBox cmbEffect)
		{
			return new StatusEffectDto
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
				case DisplayText.Deffense: ret = StatTypeDto.Defense; break;
				case DisplayText.Speed: ret = StatTypeDto.Speed; break;
				case DisplayText.CritRate: ret = StatTypeDto.CriticalRate; break;
				case DisplayText.CritDamage: ret = StatTypeDto.CriticalDamage; break;
				case DisplayText.EffectHit: ret = StatTypeDto.EffectHit; break;
				case DisplayText.EffectResist: ret = StatTypeDto.EffectResist; break;
				case DisplayText.AdditionalAttack: ret = StatTypeDto.AdditionalAttackRate; break;
				case DisplayText.AdditionalHP: ret = StatTypeDto.AdditionalHpRate; break;
				case DisplayText.AdditionalDeffense: ret = StatTypeDto.AdditionalDeffenseRate; break;
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
					if (text == DisplayText.CritDamage)
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
				case DisplayText.AdditionalAttack:
				case DisplayText.AdditionalHP:
				case DisplayText.CritRate:
				case DisplayText.EffectHit:
				case DisplayText.EffectResist:
					ret = 15.0;
					break;
				case DisplayText.CritDamage:
					ret = 20.0;
					break;
				case DisplayText.AdditionalDeffense:
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
				case DisplayText.AdditionalAttack:
				case DisplayText.AdditionalHP:
				case DisplayText.CritRate:
				case DisplayText.EffectHit:
				case DisplayText.EffectResist:
					ret = 8.0;
					break;
				case DisplayText.AdditionalDeffense:
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
			txtFinalStats.Text = formatFinalStatus(result.FinalStatus);
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
					$"{DisplayText.Deffense}: {s.Defense:F2} " +
					$"{DisplayText.Speed}: {s.Speed:F2} " +

					$"{DisplayText.AdditionalAttack}: {s.AdditionalAttackRate:F2}% " +
					$"{DisplayText.AdditionalHP}: {s.AdditionalHpRate:F2}% " +
					$"{DisplayText.AdditionalDeffense}: {s.AdditionalDeffenseRate:F2}% " +

					$"{DisplayText.CritRate}: {s.CritRate:F2}% " +
					$"{DisplayText.CritDamage}: {s.CritDamage:F2}% " +
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
					$"{DisplayText.Deffense}: {s.Defense:F2} " +
					$"{DisplayText.Speed}: {s.Speed:F2} " +
					$"{DisplayText.CritRate}: {s.CritRate:F2}% " +
					$"{DisplayText.CritDamage}: {s.CritDamage:F2}% " +
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
			initializeShikigamiComboBox();
			initializeMainStatComboBoxes();
			initializeSubStatComboBoxes();
			initializeSetEffectComboBoxes();
			initializeUniqueEffectComboBoxes();
		}

		private void initializeShikigamiComboBox()
		{
			List<ShikigamiDto> list;

			var outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out list);

			if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ読み込み"))
			{
				list = new List<ShikigamiDto>();
			}

			cmbShikigami.DataSource = list;
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
				DisplayText.AdditionalAttack,
				DisplayText.AdditionalHP,
				DisplayText.AdditionalDeffense);

			setComboItems(cmbMainStat3,
				DisplayText.Deffense);

			setComboItems(cmbMainStat4,
				DisplayText.EffectHit,
				DisplayText.EffectResist,
				DisplayText.AdditionalAttack,
				DisplayText.AdditionalHP,
				DisplayText.AdditionalDeffense);

			setComboItems(cmbMainStat5,
				DisplayText.HP);

			setComboItems(cmbMainStat6,
				DisplayText.CritRate,
				DisplayText.CritDamage,
				DisplayText.AdditionalAttack,
				DisplayText.AdditionalHP,
				DisplayText.AdditionalDeffense);
		}

		private void initializeSubStatComboBoxes()
		{
			foreach (MitamaSlotInputControl slot in getMitamaSlotInputControls())
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					setComboItems(subStat.TypeComboBox,
						DisplayText.None,
						DisplayText.Attack,
						DisplayText.HP,
						DisplayText.Deffense,
						DisplayText.Speed,
						DisplayText.CritRate,
						DisplayText.CritDamage,
						DisplayText.EffectHit,
						DisplayText.EffectResist,
						DisplayText.AdditionalAttack,
						DisplayText.AdditionalHP,
						DisplayText.AdditionalDeffense);
				}
			}
		}

		private void initializeSetEffectComboBoxes()
		{
			foreach (ComboBox comboBox in getSetEffectComboBoxes())
			{
				setComboItems(comboBox,
					DisplayText.None,
					DisplayText.CritRate,
					DisplayText.CritDamage,
					DisplayText.EffectHit,
					DisplayText.EffectResist,
					DisplayText.AdditionalAttack,
					DisplayText.AdditionalHP,
					DisplayText.AdditionalDeffense);
			}
		}

		private void initializeUniqueEffectComboBoxes()
		{
			foreach (ComboBox comboBox in getUniqueEffectComboBoxes())
			{
				setComboItems(comboBox,
					DisplayText.None,
					DisplayText.CritRate,
					DisplayText.EffectHit,
					DisplayText.EffectResist,
					DisplayText.AdditionalAttack,
					DisplayText.AdditionalHP,
					DisplayText.AdditionalDeffense);
			}
		}

		private void setComboItems(ComboBox comboBox, params string[] items)
		{
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
			using (var sfd = new SaveFileDialog())
			{
				sfd.Filter = createSaveFileDialogFilter();

				if (sfd.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				setSaveDataOperationButtonsEnabled(false);

				try
				{
					if (sfd.FilterIndex == 1)
					{
						var data = createCurrentBuildSaveData();
						SaveDataAccess.SaveBuild(sfd.FileName, data);
					}
					else if (sfd.FilterIndex == 2)
					{
						var data = createCurrentMitamaSetSaveData();
						SaveDataAccess.SaveMitamaSet(sfd.FileName, data);
					}
					else if (sfd.FilterIndex == 3)
					{
						string snapshotName = createSnapshotNameFromFilePath(sfd.FileName);

						var data = createCurrentCalculationSnapshotSaveData(snapshotName);
						SaveDataAccess.SaveSnapshot(sfd.FileName, data);
					}
				}
				finally
				{
					setSaveDataOperationButtonsEnabled(true);
				}
			}
		}

		private string createSaveFileDialogFilter()
		{
			string filter =
				"ビルド保存データ (*.build.json)|*.build.json|" +
				"御魂セット保存データ (*.mitama.json)|*.mitama.json";

			if (canSaveCalculationSnapshot())
			{
				filter += "|計算結果スナップショット (*.snapshot.json)|*.snapshot.json";
			}

			return filter;
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

		private MitamaSaveData createMitamaSaveData(
			int slot,
			MitamaSlotInputControl input)
		{
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
			double.TryParse(txt.Text, out double value);

			return new EffectSaveData
			{
				Type = cmb.Text,
				Value = value
			};
		}

		private EffectSaveData createEffectSaveData(ComboBox cmb)
		{
			return new EffectSaveData
			{
				Type = cmb.Text,
				Value = 0.0
			};
		}

		/****************************************************************************************************
		  SaveData適用
		****************************************************************************************************/
		private void btnLoad_Click(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{

				ofd.Filter =
					"ビルド保存データ (*.build.json)|*.build.json|" +
					"御魂セット保存データ (*.mitama.json)|*.mitama.json";

				if (ofd.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				setSaveDataOperationButtonsEnabled(false);

				try
				{
					if (ofd.FilterIndex == 1)
					{
						var data = SaveDataAccess.LoadBuild(ofd.FileName);
						applyBuildSaveDataToUI(data);
					}
					else if (ofd.FilterIndex == 2)
					{
						var data = SaveDataAccess.LoadMitamaSet(ofd.FileName);
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

			foreach (var item in cmbShikigami.Items)
			{
				var shikigami = item as ShikigamiDto;

				if (shikigami != null && shikigami.Name == name)
				{
					cmbShikigami.SelectedItem = shikigami;
					return;
				}
			}

			MessageBox.Show($"式神が見つかりません： {name}");
		}

		private void applyMitama(List<MitamaSaveData> list)
		{
			MitamaSlotInputControl[] slots = getMitamaSlotInputControls();

			for (int i = 0; i < list.Count && i < slots.Length; i++)
			{
				applySingleMitama(list[i], slots[i]);
			}
		}

		private void applySingleMitama(
			MitamaSaveData data,
			MitamaSlotInputControl slot)
		{
			slot.MainStatComboBox.Text = data.MainStat.Type;
			slot.MainValueTextBox.Text = data.MainStat.Value.ToString();

			for (int i = 0; i < data.SubStats.Count && i < slot.SubStats.Length; i++)
			{
				applyEffect(data.SubStats[i], slot.SubStats[i].TypeComboBox, slot.SubStats[i].ValueTextBox);
			}
		}

		private void applySetEffect(List<EffectSaveData> list)
		{
			ComboBox[] comboBoxes = getSetEffectComboBoxes();

			for (int i = 0; i < list.Count && i < comboBoxes.Length; i++)
			{
				applyEffect(list[i], comboBoxes[i]);
			}
		}

		private void applyUniqueEffect(List<EffectSaveData> list)
		{
			ComboBox[] comboBoxes = getUniqueEffectComboBoxes();

			for (int i = 0; i < list.Count && i < comboBoxes.Length; i++)
			{
				applyEffect(list[i], comboBoxes[i]);
			}
		}

		private void applyEffect(EffectSaveData data, ComboBox comboBox, TextBox textBox)
		{
			comboBox.SelectedItem = data.Type;
			textBox.Text = data.Value.ToString();
		}

		private void applyEffect(EffectSaveData data, ComboBox comboBox)
		{
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

			if (fileName.EndsWith(".snapshot.json"))
			{
				return fileName.Substring(0, fileName.Length - ".snapshot.json".Length);
			}

			return Path.GetFileNameWithoutExtension(filePath);
		}

		private CalculationSnapshotSaveData createCurrentCalculationSnapshotSaveData(string snapshotName)
		{
			return new CalculationSnapshotSaveData
			{
				SnapshotName = snapshotName,
				CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
				ShikigamiName = cmbShikigami.Text,
				MitamaSet = createCurrentMitamaSetSaveData(),
				MitamaStatus = createStatusSavedata(_lastCalculationResult.MitamaOnlyStatus),
				FinalStatus = createStatusSavedata(_lastCalculationResult.FinalStatus)
			};
		}

		private StatusSaveData createStatusSavedata(StatusDto status)
		{
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
		  式神登録
		****************************************************************************************************/
		private void btnAddShikigami_Click(object sender, EventArgs e)
		{
			var selectedShikigami = cmbShikigami.SelectedItem as ShikigamiDto;

			using (var form = new ShikigamiResisterForm())
			{
				if (form.ShowDialog() == DialogResult.OK)
				{
					initializeShikigamiComboBox();
					applyShikigami(selectedShikigami.Name);
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
				if (form.ShowDialog() == DialogResult.OK)
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
			txtBaseStats.Text = "";
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
				form.ShowDialog();
			}
		}
	}
}
