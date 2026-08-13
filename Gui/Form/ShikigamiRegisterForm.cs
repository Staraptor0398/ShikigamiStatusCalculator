using Gui.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gui.Form
{
	public partial class ShikigamiRegisterForm : System.Windows.Forms.Form
	{

		/****************************************************************************************************
		  型定義
		****************************************************************************************************/
		private enum ShikigamiFormMode
		{
			Register,
			Edit
		}

		/****************************************************************************************************
		  フィールド・プロパティ
		****************************************************************************************************/
		private readonly ShikigamiFormMode mMode;

		private readonly ShikigamiDto mEditTarget;

		public ShikigamiDto EditedShikigami { get; private set; }

		/****************************************************************************************************
		  コンストラクタ
		****************************************************************************************************/
		public ShikigamiRegisterForm()
		{
			InitializeComponent();

			mMode = ShikigamiFormMode.Register;
			mEditTarget = null;
		}

		public ShikigamiRegisterForm(ShikigamiDto editTarget)
		{
			InitializeComponent();

			mMode = ShikigamiFormMode.Edit;
			mEditTarget = editTarget;
		}

		/****************************************************************************************************
		  初期化
		****************************************************************************************************/
		private void ShikigamiRegisterForm_Load(object sender, EventArgs e)
		{
			initializeRarityComboBox();

			if (mMode == ShikigamiFormMode.Register)
			{
				initializeRegisterMode();
			}
			else
			{
				initializeEditMode();
			}
		}

		private void initializeRarityComboBox()
		{
			cmbRarity.Items.Clear();

			cmbRarity.Items.Add(DisplayText.RarityUR);
			cmbRarity.Items.Add(DisplayText.RaritySP);
			cmbRarity.Items.Add(DisplayText.RaritySSR);
			cmbRarity.Items.Add(DisplayText.RaritySR);

			cmbRarity.SelectedIndex = -1;
		}

		private void initializeRegisterMode()
		{
			this.Text = "式神登録";
			btnRegister.Text = "登録";
		}

		private void initializeEditMode()
		{
			this.Text = "式神編集";
			btnRegister.Text = "更新";

			if (mEditTarget == null)
			{
				return;
			}

			cmbRarity.SelectedItem = mEditTarget.Rarity;
			txtName.Text = mEditTarget.Name;

			txtAttack.Text = mEditTarget.Status.Attack.ToString();
			txtHP.Text = mEditTarget.Status.HP.ToString();
			txtDefense.Text = mEditTarget.Status.Defense.ToString();
			txtSpeed.Text = mEditTarget.Status.Speed.ToString();

			txtCritRate.Text = mEditTarget.Status.CritRate.ToString();
			txtCritDamage.Text = mEditTarget.Status.CritDamage.ToString();
			txtEffectHit.Text = mEditTarget.Status.EffectHit.ToString();
			txtEffectResist.Text = mEditTarget.Status.EffectResist.ToString();
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

			if (mMode == ShikigamiFormMode.Register)
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

			ShikigamiDataFileManager.CreateBackup();

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
			if (mEditTarget == null)
			{
				Logger.Error("式神データ編集に失敗しました。編集対象の式神データがnullです。");

				MessageBox.Show("編集対象の式神データが取得できませんでした。",
					"式神データ編集",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);

				return;
			}

			var dupplicateOutcome = validateDupplicateForEdit(mEditTarget, newDto);

			if (ShikigamiDataErrorHandler.Handle(dupplicateOutcome, "式神データ編集"))
			{
				return;
			}

			ShikigamiDataFileManager.CreateBackup();

			var outcome = ShikigamiGateway.UpdateShikigami(AppPath.ShikigamiDataCsvPath, mEditTarget, newDto);

			if (ShikigamiDataErrorHandler.Handle(outcome, "式神データ編集"))
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

			if (!tryGetDouble(txtAttack, DisplayText.Attack, out double attack))
			{
				return false;
			}

			if (!tryGetDouble(txtHP, DisplayText.HP, out double hp))
			{
				return false;
			}

			if (!tryGetDouble(txtDefense, DisplayText.Defense, out double deffense))
			{
				return false;
			}

			if (!tryGetDouble(txtSpeed, DisplayText.Speed, out double speed))
			{
				return false;
			}

			if (!tryGetDouble(txtCritRate, DisplayText.CriticalRate, out double critRate))
			{
				return false;
			}

			if (!tryGetDouble(txtCritDamage, DisplayText.CriticalDamage, out double critDamage))
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

				Status = new StatusDto
				{
					Attack = attack,
					HP = hp,
					Defense = deffense,
					Speed = speed,
					CritRate = critRate,
					CritDamage = critDamage,
					EffectHit = effectHit,
					EffectResist = effectResist
				}
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

		private void showInputWarning(string message, System.Windows.Forms.Control focusControl)
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
