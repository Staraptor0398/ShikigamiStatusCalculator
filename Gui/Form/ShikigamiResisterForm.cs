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
		public ShikigamiResisterForm()
		{
			InitializeComponent();
		}

		private void btnResister_Click(object sender, EventArgs e)
		{
			if(!tryBuildSikigamiDto(out ShikigamiDto dto))
			{
				return;
			}

			if (existsSameShikigami(dto))
			{
				MessageBox.Show("同じレアリティ・同じ式神名のデータが既に存在します。",
					"登録確認",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return;
			}

			ShikigamiGateway.AddShikigami(AppPath.ShikigamiDataCsvPath, dto);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void ShikigamiResisterForm_Load(object sender, EventArgs e)
		{
			initializeRarityComboBox();
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
			var list = ShikigamiGateway.GetShikigamiList(AppPath.ShikigamiDataCsvPath);

			foreach (var s in list)
			{
				if(s.Rarity == dto.Rarity && s.Name == dto.Name)
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