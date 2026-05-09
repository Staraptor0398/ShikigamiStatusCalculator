using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

using Gui.Common;
using Gui.IO;
using Gui.SaveData;

namespace ShikigamiApp
{
	public partial class MainForm : Form
	{
		private CalculationResultDto _lastCalculationResult;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			initializeComboBoxes();
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
		}

		/****************************************************************************************************
		  ステータス計算
		****************************************************************************************************/
		private void btnCalc_Click(object sender, EventArgs e)
		{
			var baseStatus = getSelectedShikigamiStatus();
			var mitamaSet = buildMitamaSetDto();

			_lastCalculationResult = CalculationGateway.calclutate(baseStatus, mitamaSet);

			showCalculationResult(_lastCalculationResult);
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
		  Dto作成
		****************************************************************************************************/
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

		private MitamaSetDto buildMitamaSetDto()
		{
			var dto = new MitamaSetDto();

			dto.Mitamas = new List<MitamaDto>
			{
				createMitamaDto(1, cmbMainStat1,
					cmbSubStat11, txtSubVal11,
					cmbSubStat21, txtSubVal21,
					cmbSubStat31, txtSubVal31,
					cmbSubStat41, txtSubVal41),

				createMitamaDto(2, cmbMainStat2,
					cmbSubStat12, txtSubVal12,
					cmbSubStat22, txtSubVal22,
					cmbSubStat32, txtSubVal32,
					cmbSubStat42, txtSubVal42),

				createMitamaDto(3, cmbMainStat3,
					cmbSubStat13, txtSubVal13,
					cmbSubStat23, txtSubVal23,
					cmbSubStat33, txtSubVal33,
					cmbSubStat43, txtSubVal43),

				createMitamaDto(4, cmbMainStat4,
					cmbSubStat14, txtSubVal14,
					cmbSubStat24, txtSubVal24,
					cmbSubStat34, txtSubVal34,
					cmbSubStat44, txtSubVal44),

				createMitamaDto(5, cmbMainStat5,
					cmbSubStat15, txtSubVal15,
					cmbSubStat25, txtSubVal25,
					cmbSubStat35, txtSubVal35,
					cmbSubStat45, txtSubVal45),

				createMitamaDto(6, cmbMainStat6,
					cmbSubStat16, txtSubVal16,
					cmbSubStat26, txtSubVal26,
					cmbSubStat36, txtSubVal36,
					cmbSubStat46, txtSubVal46)
			};

			dto.SetEffects = new List<StatusEffectDto>
			{
				createSetEffectDto(cmbSetBonus1),
				createSetEffectDto(cmbSetBonus2),
				createSetEffectDto(cmbSetBonus3)
			};

			dto.UniqueEffects = new List<StatusEffectDto>
			{
				createUniqueEffectDto(cmbUnique1),
				createUniqueEffectDto(cmbUnique2),
				createUniqueEffectDto(cmbUnique3),
				createUniqueEffectDto(cmbUnique4),
				createUniqueEffectDto(cmbUnique5),
				createUniqueEffectDto(cmbUnique6),
			};

			return dto;
		}

		private MitamaDto createMitamaDto(
			int slot,
			ComboBox cmbMainType,
			ComboBox cmbSubType1, TextBox textSubValue1,
			ComboBox cmbSubType2, TextBox textSubValue2,
			ComboBox cmbSubType3, TextBox textSubValue3,
			ComboBox cmbSubType4, TextBox textSubValue4)
		{
			var dto = new MitamaDto();

			dto.MainStat = new StatValueDto
			{
				Type = convertStatType(cmbMainType.Text),
				Value = getMainStatValue(cmbMainType.Text, slot)
			};

			dto.SubStat = new List<StatValueDto>
			{
				createStatValueDto(cmbSubType1,textSubValue1),
				createStatValueDto(cmbSubType2,textSubValue2),
				createStatValueDto(cmbSubType3,textSubValue3),
				createStatValueDto(cmbSubType4,textSubValue4)
			};

			return dto;
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
		}

		/****************************************************************************************************
		  ComboBox初期化
		****************************************************************************************************/
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
			ComboBox[] comboBoxes =
			{
				cmbSubStat11,   cmbSubStat21,   cmbSubStat31,   cmbSubStat41,
				cmbSubStat12,   cmbSubStat22,   cmbSubStat32,   cmbSubStat42,
				cmbSubStat13,   cmbSubStat23,   cmbSubStat33,   cmbSubStat43,
				cmbSubStat14,   cmbSubStat24,   cmbSubStat34,   cmbSubStat44,
				cmbSubStat15,   cmbSubStat25,   cmbSubStat35,   cmbSubStat45,
				cmbSubStat16,   cmbSubStat26,   cmbSubStat36,   cmbSubStat46,
			};

			foreach (ComboBox comboBox in comboBoxes)
			{
				setComboItems(comboBox,
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

		private void initializeSetEffectComboBoxes()
		{
			ComboBox[] comboBoxes =
			{
				cmbSetBonus1,
				cmbSetBonus2,
				cmbSetBonus3
			};

			foreach (ComboBox comboBox in comboBoxes)
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
			ComboBox[] comboBoxes =
			{
				cmbUnique1,
				cmbUnique2,
				cmbUnique3,
				cmbUnique4,
				cmbUnique5,
				cmbUnique6
			};

			foreach (ComboBox comboBox in comboBoxes)
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

		private void initializeComboBoxes()
		{
			initializeShikigamiComboBox();
			initializeMainStatComboBoxes();
			initializeSubStatComboBoxes();
			initializeSetEffectComboBoxes();
			initializeUniqueEffectComboBoxes();
		}

		/****************************************************************************************************
		  メインステータス表示
		****************************************************************************************************/
		private void cmbMainStat1_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat1.SelectedItem.ToString(), 1);
			txtMainVal1.Text = value.ToString();
		}

		private void cmbMainStat2_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat2.SelectedItem.ToString(), 2);
			txtMainVal2.Text = value.ToString();
		}

		private void cmbMainStat3_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat3.SelectedItem.ToString(), 3);
			txtMainVal3.Text = value.ToString();
		}

		private void cmbMainStat4_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat4.SelectedItem.ToString(), 4);
			txtMainVal4.Text = value.ToString();
		}

		private void cmbMainStat5_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat5.SelectedItem.ToString(), 5);
			txtMainVal5.Text = value.ToString();
		}

		private void cmbMainStat6_SelectedIndexChanged(object sender, EventArgs e)
		{
			var value = getMainStatValue(cmbMainStat6.SelectedItem.ToString(), 6);
			txtMainVal6.Text = value.ToString();
		}

		/****************************************************************************************************
		  SaveData保存
		****************************************************************************************************/
		private void btnSave_Click(object sender, EventArgs e)
		{
			using (var sfd = new SaveFileDialog())
			{
				sfd.Filter =
					"ビルド保存データ (*.build.json)|*.build.json|" +
					"御魂セット保存データ (*.mitama.json)|*.mitama.json";


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

		private MitamaSaveData createMitamaSaveData(
			int slot,
			ComboBox cmbMain,
			ComboBox cmbSub1, TextBox txt1,
			ComboBox cmbSub2, TextBox txt2,
			ComboBox cmbSub3, TextBox txt3,
			ComboBox cmbSub4, TextBox txt4)
		{
			return new MitamaSaveData
			{
				Slot = slot,
				MainStat = new EffectSaveData
				{
					Type = cmbMain.Text,
					Value = getMainStatValue(cmbMain.Text, slot)
				},
				SubStats = new List<EffectSaveData>
				{
					createEffectSaveData(cmbSub1,txt1),
					createEffectSaveData(cmbSub2,txt2),
					createEffectSaveData(cmbSub3,txt3),
					createEffectSaveData(cmbSub4,txt4)
				}

			};
		}

		private List<MitamaSaveData> createMitamaSaveDataList()
		{
			return new List<MitamaSaveData>
			{
				createMitamaSaveData(1, cmbMainStat1,
					cmbSubStat11, txtSubVal11,
					cmbSubStat21, txtSubVal21,
					cmbSubStat31, txtSubVal31,
					cmbSubStat41, txtSubVal41),

				createMitamaSaveData(2, cmbMainStat2,
					cmbSubStat12, txtSubVal12,
					cmbSubStat22, txtSubVal22,
					cmbSubStat32, txtSubVal32,
					cmbSubStat42, txtSubVal42),

				createMitamaSaveData(3, cmbMainStat3,
					cmbSubStat13, txtSubVal13,
					cmbSubStat23, txtSubVal23,
					cmbSubStat33, txtSubVal33,
					cmbSubStat43, txtSubVal43),

				createMitamaSaveData(4, cmbMainStat4,
					cmbSubStat14, txtSubVal14,
					cmbSubStat24, txtSubVal24,
					cmbSubStat34, txtSubVal34,
					cmbSubStat44, txtSubVal44),

				createMitamaSaveData(5, cmbMainStat5,
					cmbSubStat15, txtSubVal15,
					cmbSubStat25, txtSubVal25,
					cmbSubStat35, txtSubVal35,
					cmbSubStat45, txtSubVal45),

				createMitamaSaveData(6, cmbMainStat6,
					cmbSubStat16, txtSubVal16,
					cmbSubStat26, txtSubVal26,
					cmbSubStat36, txtSubVal36,
					cmbSubStat46, txtSubVal46),
			};
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

		private List<EffectSaveData> createSetEffectSaveData()
		{
			return new List<EffectSaveData>
			{
				createEffectSaveData(cmbSetBonus1, new TextBox()),
				createEffectSaveData(cmbSetBonus2, new TextBox()),
				createEffectSaveData(cmbSetBonus3 , new TextBox())
			};
		}

		private List<EffectSaveData> createUniqueEffectSaveData()
		{
			return new List<EffectSaveData>
			{
				createEffectSaveData(cmbUnique1, new TextBox()),
				createEffectSaveData(cmbUnique2, new TextBox()),
				createEffectSaveData(cmbUnique3, new TextBox()),
				createEffectSaveData(cmbUnique4, new TextBox()),
				createEffectSaveData(cmbUnique5, new TextBox()),
				createEffectSaveData(cmbUnique6, new TextBox())
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
				}
			}
		}

		private void applyEffect(EffectSaveData e, ComboBox cmb, TextBox txt)
		{
			if (e == null)
			{
				return;
			}

			cmb.SelectedItem = e.Type;
			txt.Text = e.Value.ToString();
		}

		private void applySetEffect(List<EffectSaveData> list)
		{
			if (list == null)
			{
				return;
			}

			cmbSetBonus1.SelectedItem = list.ElementAtOrDefault(0)?.Type ?? "";
			cmbSetBonus2.SelectedItem = list.ElementAtOrDefault(1)?.Type ?? "";
			cmbSetBonus3.SelectedItem = list.ElementAtOrDefault(2)?.Type ?? "";
		}

		private void applyUniqueEffect(List<EffectSaveData> list)
		{
			if (list == null)
			{
				return;
			}

			cmbUnique1.SelectedItem = list.ElementAtOrDefault(0)?.Type ?? "";
			cmbUnique2.SelectedItem = list.ElementAtOrDefault(1)?.Type ?? "";
			cmbUnique3.SelectedItem = list.ElementAtOrDefault(2)?.Type ?? "";
			cmbUnique4.SelectedItem = list.ElementAtOrDefault(3)?.Type ?? "";
			cmbUnique5.SelectedItem = list.ElementAtOrDefault(4)?.Type ?? "";
			cmbUnique6.SelectedItem = list.ElementAtOrDefault(5)?.Type ?? "";
		}

		private void applySingleMitama(
			MitamaSaveData m,
			ComboBox cmbMain,
			ComboBox cmbSub1, TextBox txt1,
			ComboBox cmbSub2, TextBox txt2,
			ComboBox cmbSub3, TextBox txt3,
			ComboBox cmbSub4, TextBox txt4)
		{
			if (m == null)
			{
				return;
			}

			cmbMain.SelectedItem = m.MainStat?.Type ?? "";

			applyEffect(m.SubStats?.ElementAtOrDefault(0), cmbSub1, txt1);
			applyEffect(m.SubStats?.ElementAtOrDefault(1), cmbSub2, txt2);
			applyEffect(m.SubStats?.ElementAtOrDefault(2), cmbSub3, txt3);
			applyEffect(m.SubStats?.ElementAtOrDefault(3), cmbSub4, txt4);
		}

		private void applyMitama(List<MitamaSaveData> list)
		{
			if (list == null)
			{
				return;
			}

			foreach (var m in list)
			{
				switch (m.Slot)
				{
					case 1:
						applySingleMitama(m,
							cmbMainStat1,
							cmbSubStat11, txtSubVal11,
							cmbSubStat21, txtSubVal21,
							cmbSubStat31, txtSubVal31,
							cmbSubStat41, txtSubVal41);
						break;
					case 2:
						applySingleMitama(m,
							cmbMainStat2,
							cmbSubStat12, txtSubVal12,
							cmbSubStat22, txtSubVal22,
							cmbSubStat32, txtSubVal32,
							cmbSubStat42, txtSubVal42);
						break;
					case 3:
						applySingleMitama(m,
							cmbMainStat3,
							cmbSubStat13, txtSubVal13,
							cmbSubStat23, txtSubVal23,
							cmbSubStat33, txtSubVal33,
							cmbSubStat43, txtSubVal43);
						break;
					case 4:
						applySingleMitama(m,
							cmbMainStat4,
							cmbSubStat14, txtSubVal14,
							cmbSubStat24, txtSubVal24,
							cmbSubStat34, txtSubVal34,
							cmbSubStat44, txtSubVal44);
						break;
					case 5:
						applySingleMitama(m,
							cmbMainStat5,
							cmbSubStat15, txtSubVal15,
							cmbSubStat25, txtSubVal25,
							cmbSubStat35, txtSubVal35,
							cmbSubStat45, txtSubVal45);
						break;
					case 6:
						applySingleMitama(m,
							cmbMainStat6,
							cmbSubStat16, txtSubVal16,
							cmbSubStat26, txtSubVal26,
							cmbSubStat36, txtSubVal36,
							cmbSubStat46, txtSubVal46);
						break;
					default:
						break;
				}
			}
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

		private void setSaveDataOperationButtonsEnabled(bool enabled)
		{
			btnSave.Enabled = enabled;
			btnLoad.Enabled = enabled;
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
			TextBox[] textBoxes =
			{
				txtMainVal1,
				txtMainVal2,
				txtMainVal3,
				txtMainVal4,
				txtMainVal5,
				txtMainVal6
			};

			foreach (var textBox in textBoxes)
			{
				textBox.Text = "";
			}
		}

		private void clearSubValueTextBoxes()
		{
			TextBox[] textBoxes =
			{
				txtSubVal11,    txtSubVal21,    txtSubVal31,    txtSubVal41,
				txtSubVal12,    txtSubVal22,    txtSubVal32,    txtSubVal42,
				txtSubVal13,    txtSubVal23,    txtSubVal33,    txtSubVal43,
				txtSubVal14,    txtSubVal24,    txtSubVal34,    txtSubVal44,
				txtSubVal15,    txtSubVal25,    txtSubVal35,    txtSubVal45,
				txtSubVal16,    txtSubVal26,    txtSubVal36,    txtSubVal46
			};

			foreach (var textBox in textBoxes)
			{
				textBox.Text = "";
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