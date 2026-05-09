using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gui.Common;

namespace ShikigamiApp
{
	public partial class ShikigamiResisterForm : Form
	{

		private enum ShikigamiFormMode
		{
			Resister,
			Edit
		}

		private readonly ShikigamiFormMode _mode;

		private readonly ShikigamiDto _editTarget;

		public ShikigamiDto EditedShikigami { get; private set; }

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

		private void initializeResisterMode()
		{
			this.Text = "式神登録";
			btnResister.Text = "登録";
		}

		private void initializeEditMode()
		{
			this.Text = "式神編集";
			btnResister.Text = "更新";

			if(_editTarget == null)
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
			txtEffectResist.Text= _editTarget.EffectResist.ToString();
		}

		private void btnResister_Click(object sender, EventArgs e)
		{
			if(!tryBuildSikigamiDto(out ShikigamiDto dto))
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
			if (existsSameShikigami(dto))
			{
				MessageBox.Show("同じレアリティ・同じ式神名のデータが既に存在します。",
					"式神データ登録",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return;
			}

			ShikigamiGateway.AddShikigami(AppPath.ShikigamiDataCsvPath, dto);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void updateShikigami(ShikigamiDto newDto)
		{
			if (_editTarget == null)
			{
				MessageBox.Show("編集対象の式神データが取得できませんでした。",
					"式神データ編集",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			if(existsSameShikigamiExceptSelf(_editTarget, newDto))
			{
				MessageBox.Show("変更後の式紙データと同じデータが既に存在します。",
					"式神データ編集",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return;
			}

			ShikigamiGateway.UpdateShikigami(AppPath.ShikigamiDataCsvPath, _editTarget, newDto);

			EditedShikigami = newDto;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void initializeRarityComboBox()
		{
			cmbRarity.Items.Clear();

			cmbRarity.Items.Add(DisplayText.RaritySP);
			cmbRarity.Items.Add(DisplayText.RaritySSR);
			cmbRarity.Items.Add(DisplayText.RaritySR);

			cmbRarity.SelectedIndex = -1;
		}

		private bool tryGetDouble(TextBox textBox,string itemName,out double value)
		{
			if(double.TryParse(textBox.Text,out value))
			{
				return true;
			}

			MessageBox.Show($"{itemName}には数値を入力してください。");
			textBox.Focus();
			return false;
		}

		private bool tryBuildSikigamiDto(out ShikigamiDto dto)
		{
			dto = null;

			if (string.IsNullOrWhiteSpace(cmbRarity.Text))
			{
				MessageBox.Show("レアリティを選択してください。");
				cmbRarity.Focus();
				return false;
			}

			if (string.IsNullOrWhiteSpace(txtName.Text))
			{
				MessageBox.Show("式紙名を入力してください。");
				txtName.Focus();
				return false;
			}

			if(!tryGetDouble(txtAttck,DisplayText.Attack,out double attack))
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

		private bool existsSameShikigami(ShikigamiDto dto)
		{
			List<ShikigamiDto> list;

			ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out list);

			foreach (var s in list)
			{
				if (s.Rarity == dto.Rarity && s.Name == dto.Name)
				{
					return true;
				}
			}

			return false;
		}

		private bool existsSameShikigamiExceptSelf(ShikigamiDto oldDto, ShikigamiDto newDto)
		{
			List<ShikigamiDto> list;

			ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath, out list);

			foreach (var s in list)
			{
				if (s.Rarity == oldDto.Rarity && s.Name == oldDto.Name)
				{
					continue;
				}

				if (s.Rarity == newDto.Rarity && s.Name == newDto.Name)
				{
					return true;
				}
			}

			return false;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}