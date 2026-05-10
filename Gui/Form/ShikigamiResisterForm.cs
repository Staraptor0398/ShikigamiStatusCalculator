using Gui.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ShikigamiApp
{
	public partial class ShikigamiResisterForm : Form
	{

		/****************************************************************************************************
		  型定義
		****************************************************************************************************/
		private enum ShikigamiFormMode
		{
			Resister,
			Edit
		}

		/****************************************************************************************************
		  フィールド・プロパティ
		****************************************************************************************************/
		private readonly ShikigamiFormMode _mode;

		private readonly ShikigamiDto _editTarget;

		public ShikigamiDto EditedShikigami { get; private set; }

		/****************************************************************************************************
		  コンストラクタ
		****************************************************************************************************/
		public ShikigamiResisterForm()
		{
			InitializeComponent();

			_mode = ShikigamiFormMode.Resister;
			_editTarget = null;
		}

		public ShikigamiResisterForm(ShikigamiDto editTarget)
		{
			InitializeComponent();

			_mode = ShikigamiFormMode.Edit;
			_editTarget = editTarget;
		}

		/****************************************************************************************************
		  初期化
		****************************************************************************************************/
		private void ShikigamiResisterForm_Load(object sender, EventArgs e)
		{
			initializeRarityComboBox();

			if (_mode == ShikigamiFormMode.Resister)
			{
				initializeResisterMode();
			}
			else
			{
				initializeEditMode();
			}
		}

		private void initializeRarityComboBox()
		{
			cmbRarity.Items.Clear();

			cmbRarity.Items.Add(DisplayText.RaritySP);
			cmbRarity.Items.Add(DisplayText.RaritySSR);
			cmbRarity.Items.Add(DisplayText.RaritySR);

			cmbRarity.SelectedIndex = -1;
		}

		private void initializeResisterMode()
		{
			this.Text = "式神登録";
			btnResister.Text = "登録";
		}

		private void initializeEditMode()
		{
			this.Text = "式神編集";
			btnResister.Text = "更新";

			if (_editTarget == null)
			{
				return;
			}

			cmbRarity.SelectedItem = _editTarget.Rarity;
			txtName.Text = _editTarget.Name;

			txtAttck.Text = _editTarget.Attack.ToString();
			txtHP.Text = _editTarget.HP.ToString();
			txtDeffense.Text = _editTarget.Defense.ToString();
			txtSpeed.Text = _editTarget.Speed.ToString();

			txtCritRate.Text = _editTarget.CritRate.ToString();
			txtCritDamage.Text = _editTarget.CritDamage.ToString();
			txtEffectHit.Text = _editTarget.EffectHit.ToString();
			txtEffectResist.Text = _editTarget.EffectResist.ToString();
		}

		/****************************************************************************************************
		  登録・編集
		****************************************************************************************************/
		private void btnResister_Click(object sender, EventArgs e)
		{
			if (!tryBuildSikigamiDto(out ShikigamiDto dto))
			{
				return;
			}

			if (_mode == ShikigamiFormMode.Resister)
			{
				registerShikigami(dto);
			}
			else
			{
				updateShikigami(dto);
			}
		}

		private void registerShikigami(ShikigamiDto dto)
		{
			var dupplicateOutcome = validateDupplicateForResister(dto);

			if (ShikigamiDataErrorHandler.Handle(dupplicateOutcome, "式神データ登録"))
			{
				return;
			}

			var outcome = ShikigamiGateway.AddShikigami(AppPath.ShikigamiDataCsvPath, dto);

			if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ登録"))
			{
				return;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void updateShikigami(ShikigamiDto newDto)
		{
			if (_editTarget == null)
			{
				Logger.Error("式神データ編集に失敗しました。編集対象の式神データがnullです。");

				MessageBox.Show("編集対象の式神データが取得できませんでした。",
					"式神データ編集",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);

				return;
			}

			var dupplicateOutcome = validateDupplicateForEdit(_editTarget, newDto);

			if (ShikigamiDataErrorHandler.Handle(dupplicateOutcome, "式神データ編集"))
			{
				return;
			}

			var outcome = ShikigamiGateway.UpdateShikigami(AppPath.ShikigamiDataCsvPath, _editTarget, newDto);

			if (ShikigamiDataErrorHandler.Handle(dupplicateOutcome, "式神データ編集"))
			{
				return;
			}

			EditedShikigami = newDto;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
		/****************************************************************************************************
		  入力値取得・入力チェック
		****************************************************************************************************/
		private bool tryBuildSikigamiDto(out ShikigamiDto dto)
		{
			dto = null;

			if (string.IsNullOrWhiteSpace(cmbRarity.Text))
			{
				showInputWarning("レアリティを選択してください。", cmbRarity);
				return false;
			}

			if (string.IsNullOrWhiteSpace(txtName.Text))
			{
				showInputWarning("式神名を入力してください。", txtName);
				return false;
			}

			if (!tryGetDouble(txtAttck, DisplayText.Attack, out double attack))
			{
				return false;
			}
			if (!tryGetDouble(txtHP, DisplayText.HP, out double hp))
			{
				return false;
			}
			if (!tryGetDouble(txtDeffense, DisplayText.Deffense, out double deffense))
			{
				return false;
			}
			if (!tryGetDouble(txtSpeed, DisplayText.Speed, out double speed))
			{
				return false;
			}
			if (!tryGetDouble(txtCritRate, DisplayText.CritRate, out double critRate))
			{
				return false;
			}
			if (!tryGetDouble(txtCritDamage, DisplayText.CritDamage, out double critDamage))
			{
				return false;
			}
			if (!tryGetDouble(txtEffectHit, DisplayText.EffectHit, out double effectHit))
			{
				return false;
			}
			if (!tryGetDouble(txtEffectResist, DisplayText.EffectResist, out double effectResist))
			{
				return false;
			}

			dto = new ShikigamiDto
			{
				Rarity = cmbRarity.Text,
				Name = txtName.Text.Trim(),
				Attack = attack,
				HP = hp,
				Defense = deffense,
				Speed = speed,
				CritRate = critRate,
				CritDamage = critDamage,
				EffectHit = effectHit,
				EffectResist = effectResist
			};

			return true;
		}

		private bool tryGetDouble(TextBox textBox, string itemName, out double value)
		{
			value = 0;

			if (!double.TryParse(textBox.Text, out value))
			{
				showInputWarning($"{itemName}には数値を入力してください。", textBox);
				return false;
			}

			if (value < 0)
			{
				showInputWarning($"{itemName}には0以上の値を入力してください。", textBox);
				return false;
			}

			return true;
		}

		private void showInputWarning(string message, Control focusControl)
		{
			Logger.Warning($"Operation=式神データ入力検証 Message={message}");

			MessageBox.Show(
				message,
				"式神データ入力",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);

			focusControl.Focus();
		}
		/****************************************************************************************************
		  重複チェック
		****************************************************************************************************/
		private bool isSameShikigami(ShikigamiDto left, ShikigamiDto right)
		{
			return left.Rarity == right.Rarity && left.Name == right.Name;
		}

		private ShikigamiDataOutcomeDto validateDupplicateForResister(ShikigamiDto dto)
		{
			var outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out List<ShikigamiDto> list);

			if (outcome != ShikigamiDataOutcomeDto.SUCCESS)
			{
				return outcome;
			}

			foreach (var shikigami in list)
			{
				if (isSameShikigami(shikigami, dto))
				{
					return ShikigamiDataOutcomeDto.DUPLICATE;
				}
			}

			return ShikigamiDataOutcomeDto.SUCCESS;
		}

		private ShikigamiDataOutcomeDto validateDupplicateForEdit(ShikigamiDto oldDto, ShikigamiDto newDto)
		{
			var outcome = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out List<ShikigamiDto> list);

			if (outcome != ShikigamiDataOutcomeDto.SUCCESS)
			{
				return outcome;
			}

			foreach (var shikigami in list)
			{
				// 編集対象の式神自身は重複チェックから除外する
				if (isSameShikigami(shikigami, oldDto))
				{
					continue;
				}

				if (isSameShikigami(shikigami, newDto))
				{
					return ShikigamiDataOutcomeDto.DUPLICATE;
				}
			}

			return ShikigamiDataOutcomeDto.SUCCESS;
		}
	}
}
