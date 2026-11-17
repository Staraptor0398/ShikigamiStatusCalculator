using Gui.Logging;
using System;
using System.Windows.Forms;

namespace Gui.Operator
{
	internal sealed class ComboBoxOperator
	{
		private readonly PerformanceTracker mPerformanceTracker;

		public ComboBoxOperator(PerformanceTracker performanceTracker)
		{
			if (performanceTracker == null)
			{
				throw new ArgumentNullException(nameof(performanceTracker));
			}


			mPerformanceTracker = performanceTracker;
		}

		public void SetItems(ComboBox comboBox, string targetPrefix, params string[] items)
		{
			if (comboBox == null || items == null)
			{
				return;
			}

			bool isSubStat = targetPrefix == "SubStat";

			using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.BeginUpdate") : null)
			{
				comboBox.BeginUpdate();
			}

			try
			{
				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.ClearItems") : null)
				{
					comboBox.Items.Clear();
				}

				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.AddRange") : null)
				{
					if (isSubStat)
					{
						string handleState = comboBox.IsHandleCreated ? "HandleCreated" : "HandleNotCreated";

						using (mPerformanceTracker.Track($"SubStat.AddRange.{handleState}"))
						{
							string configuration = $"Sorted{comboBox.Sorted}.{comboBox.DropDownStyle}";

							using (mPerformanceTracker.Track($"SubStat.AddRange.Configuration.{configuration}"))
							{
								comboBox.Items.AddRange(items);
							}
						}
					}
					else
					{
						comboBox.Items.AddRange(items);
					}
				}

				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.ResetSelection") : null)
				{
					comboBox.SelectedIndex = -1;
				}

				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.ClearText") : null)
				{
					comboBox.Text = "";
				}
			}
			finally
			{
				using (targetPrefix != null ? mPerformanceTracker.Track($"{targetPrefix}.EndUpdate") : null)
				{
					comboBox.EndUpdate();
				}
			}
		}

		public void Reset(ComboBox comboBox)
		{
			if (comboBox == null)
			{
				return;
			}

			comboBox.SelectedIndex = -1;
			comboBox.Text = "";
		}
	}
}
