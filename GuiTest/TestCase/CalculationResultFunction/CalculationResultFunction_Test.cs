
using Gui.ControlSet;
using Gui.Function;
using Gui.Logging;
using Gui.Operator;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace GuiTest.TestCase
{
	[TestClass]
	[DoNotParallelize]
	public class CalculationResultFunction_Test
	{
		[TestMethod]
		public void InvalidateWhileDirtyChangeSuppressed()
		{
			runOnSta(() =>
			{
				using (var mitamaTextBox = new TextBox())
				using (var finalTextBox = new TextBox())
				using (var shikigamiComboBox = new ComboBox())
				using (var rarityComboBox = new ComboBox())
				using (var baseStatsTextBox = new TextBox())
				{
					var tracker = new PerformanceTracker();
					var comboBoxOperator = new ComboBoxOperator(tracker);

					var shikigamiControls = new ShikigamiSelectionControlSet(shikigamiComboBox, rarityComboBox, baseStatsTextBox);

					var shikigamiOperator = new ShikigamiSelectionOperator(shikigamiControls, comboBoxOperator, tracker);

					var resultControls = new CalculationResultControlSet(mitamaTextBox, finalTextBox);

					var resultOperator = new CalculationResultOperator(resultControls);

					bool suppressDirty = true;
					int updateSaveButtonCount = 0;

					var function = new CalculationResultFunction(resultOperator, shikigamiOperator, () => suppressDirty, source => updateSaveButtonCount++);

					// 正常な計算終了後のClean状態を再現
					var calculationResult = new CalculationResultDto();

					function.SetResult(calculationResult);
					function.MarkClean();

					mitamaTextBox.Text = "Previous Mitama Status";
					finalTextBox.Text = "Previous Final Status";

					Assert.IsTrue(function.CanSaveCalculationSnapshot());
					Assert.AreSame(calculationResult, function.LastCalculationResult);

					// LOADING中は通常のDirty変更が抑制される
					function.MarkDirty();

					Assert.IsTrue(function.CanSaveCalculationSnapshot());
					Assert.AreSame(calculationResult, function.LastCalculationResult);

					Assert.AreEqual("Previous Mitama Status", mitamaTextBox.Text);
					Assert.AreEqual("Previous Final Status", finalTextBox.Text);

					// LOADING中でも強制無効化は実行される
					function.Invalidate();

					Assert.IsFalse(function.CanSaveCalculationSnapshot());
					Assert.IsNull(function.LastCalculationResult);

					Assert.AreEqual("", mitamaTextBox.Text);
					Assert.AreEqual("", finalTextBox.Text);

					// 強制無効化自体は保存ボタン更新処理を呼ばない
					Assert.AreEqual(0, updateSaveButtonCount);

					// LOADING終了後もSnapshot保存不可を維持
					suppressDirty = false;

					Assert.IsFalse(function.CanSaveCalculationSnapshot());
				}
			});
		}

		private static void runOnSta(Action action)
		{
			Exception failure = null;

			Thread thread = new Thread(() =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					failure = ex;
				}
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();

			if (failure != null)
			{
				ExceptionDispatchInfo.Capture(failure).Throw();
			}
		}
	}
}
