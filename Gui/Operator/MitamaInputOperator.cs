using Gui.ControlSet;
using Gui.Definition;
using Gui.Factory;
using Gui.Form.Control;
using Gui.Logging;
using Gui.Model;
using Gui.Validation;
using System;
using System.Windows.Forms;

namespace Gui.Operator
{
	internal sealed class MitamaInputOperator
	{
		private readonly MitamaInputControlSet mControls;
		private readonly ComboBoxOperator mComboBoxOperator;
		private readonly PerformanceTracker mPerformanceTracker;

		public MitamaInputOperator(MitamaInputControlSet controls, ComboBoxOperator comboBoxOperator, PerformanceTracker performanceTracker)
		{
			if (controls == null)
			{
				throw new ArgumentNullException(nameof(controls));
			}
			if (comboBoxOperator == null)
			{
				throw new ArgumentNullException(nameof(comboBoxOperator));
			}
			if (performanceTracker == null)
			{
				throw new ArgumentNullException(nameof(performanceTracker));
			}


			mControls = controls;
			mComboBoxOperator = comboBoxOperator;
			mPerformanceTracker = performanceTracker;
		}

		public void InitializeMainStatComboBoxes()
		{
			mComboBoxOperator.SetItems(mControls.Slots[0].MainStatComboBox, null,
				DisplayText.ATTACK);

			mComboBoxOperator.SetItems(mControls.Slots[1].MainStatComboBox, null,
				DisplayText.SPEED,
				DisplayText.ADDITIONAL_ATTACK_RATE,
				DisplayText.ADDITIONAL_HP_RATE,
				DisplayText.ADDITIONAL_DEFENSE_RATE);

			mComboBoxOperator.SetItems(mControls.Slots[2].MainStatComboBox, null,
				DisplayText.DEFENSE);

			mComboBoxOperator.SetItems(mControls.Slots[3].MainStatComboBox, null,
				DisplayText.EFFECT_HIT,
				DisplayText.EFFECT_RESIST,
				DisplayText.ADDITIONAL_ATTACK_RATE,
				DisplayText.ADDITIONAL_HP_RATE,
				DisplayText.ADDITIONAL_DEFENSE_RATE);

			mComboBoxOperator.SetItems(mControls.Slots[4].MainStatComboBox, null,
				DisplayText.HP);

			mComboBoxOperator.SetItems(mControls.Slots[5].MainStatComboBox, null,
				DisplayText.CRITICAL_RATE,
				DisplayText.CRITICAL_DAMAGE,
				DisplayText.ADDITIONAL_ATTACK_RATE,
				DisplayText.ADDITIONAL_HP_RATE,
				DisplayText.ADDITIONAL_DEFENSE_RATE);
		}

		public void InitializeSubStatComboBoxes()
		{
			foreach (MitamaSlotInputControl slot in mControls.Slots)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					using (mPerformanceTracker.Track("SubStat.SetItems"))
					{
						mComboBoxOperator.SetItems(subStat.TypeComboBox, "SubStat",
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

		public void InitializeSetEffectComboBoxes()
		{
			foreach (ComboBox comboBox in mControls.SetEffects)
			{
				mComboBoxOperator.SetItems(comboBox, "SetEffect",
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

		public void InitializeUniqueEffectComboBoxes()
		{
			foreach (ComboBox comboBox in mControls.UniqueEffects)
			{
				mComboBoxOperator.SetItems(comboBox, "UniqueEffect",
					DisplayText.NONE,
					DisplayText.CRITICAL_RATE,
					DisplayText.EFFECT_HIT,
					DisplayText.EFFECT_RESIST,
					DisplayText.ADDITIONAL_ATTACK_RATE,
					DisplayText.ADDITIONAL_HP_RATE,
					DisplayText.ADDITIONAL_DEFENSE_RATE);
			}
		}

		public void RegisterInputChangedEvents(EventHandler handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			foreach (MitamaSlotInputControl slot in mControls.Slots)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					subStat.TypeComboBox.SelectedIndexChanged += handler;
					subStat.ValueTextBox.TextChanged += handler;
				}
			}

			foreach (ComboBox comboBox in mControls.SetEffects)
			{
				comboBox.SelectedIndexChanged += handler;
			}

			foreach (ComboBox comboBox in mControls.UniqueEffects)
			{
				comboBox.SelectedIndexChanged += handler;
			}
		}

		public string GetInputChangeSource(object sender)
		{
			if (sender is System.Windows.Forms.Control control)
			{
				if (control.Name.StartsWith("cmbSubStat", StringComparison.Ordinal))
				{
					return "Input.SubStatType";
				}

				if (control.Name.StartsWith("txtSubVal", StringComparison.Ordinal))
				{
					return "Input.SubStatValue";
				}

				if (control.Name.StartsWith("cmbSetBonus", StringComparison.Ordinal))
				{
					return "Input.SetEffect";
				}

				if (control.Name.StartsWith("cmbUnique", StringComparison.Ordinal))
				{
					return "Input.UniqueEffect";
				}
			}

			return "Input.Other";
		}

		public string GetSelectedMainStatText(int slot)
		{
			return getSlot(slot).MainStatComboBox.SelectedItem?.ToString();
		}

		public void SetMainStatValueText(int slot, string valueText)
		{
			getSlot(slot).MainValueTextBox.Text = valueText;
		}

		public void Clear()
		{
			foreach (MitamaSlotInputControl slot in mControls.Slots)
			{
				mComboBoxOperator.Reset(slot.MainStatComboBox);
			}

			foreach (MitamaSlotInputControl slot in mControls.Slots)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					mComboBoxOperator.Reset(subStat.TypeComboBox);
				}
			}

			foreach (MitamaSlotInputControl slot in mControls.Slots)
			{
				slot.MainValueTextBox.Text = "";
			}

			foreach (MitamaSlotInputControl slot in mControls.Slots)
			{
				foreach (SubStatInputControl subStat in slot.SubStats)
				{
					subStat.ValueTextBox.Text = "";
				}
			}

			foreach (ComboBox comboBox in mControls.SetEffects)
			{
				mComboBoxOperator.Reset(comboBox);
			}

			foreach (ComboBox comboBox in mControls.UniqueEffects)
			{
				mComboBoxOperator.Reset(comboBox);
			}
		}

		public MitamaSetInputModel CreateInputModel()
		{
			return MitamaSetInputModelFactory.Create(mControls.Slots, mControls.SetEffects, mControls.UniqueEffects);
		}

		public void ValidateInputModel(MitamaSetInputModel inputModel)
		{
			SaveDataInputModelValidator.Validate(inputModel, mControls.Slots, mControls.SetEffects, mControls.UniqueEffects);
		}

		private MitamaSlotInputControl getSlot(int slot)
		{
			if (slot < 1 || slot > mControls.Slots.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(slot));
			}

			return mControls.Slots[slot - 1];
		}
	}
}
