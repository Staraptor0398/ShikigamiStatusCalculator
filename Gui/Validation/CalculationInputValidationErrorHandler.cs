using Gui.Common;
using System.Windows.Forms;

namespace Gui.Validation
{
	public static class CalculationInputValidationErrorHandler
	{
		public static bool Handle(CalculationInputValidationOutcome outcome)
		{
			if (outcome == CalculationInputValidationOutcome.SUCCESS)
			{
				return false;
			}

			string message = createMessage(outcome);
			string logMessage = message.Replace("\n\n", " ");

			Logger.Warning($"Operation=ステータス計算入力検証 Outcome={outcome} Message={logMessage}");

			MessageBox.Show(
				message,
				"ステータス計算",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);

			return true;
		}

		private static string createMessage(CalculationInputValidationOutcome outcome)
		{
			switch (outcome)
			{
				case CalculationInputValidationOutcome.NO_EQUIPPED_MITAMA:
					return "ステータス計算に失敗しました。\n\n" +
							"御魂が1つも装備されていません。";
				case CalculationInputValidationOutcome.MAIN_STAT_NOT_SELECTED_WITH_SUB_STAT:
					return "ステータス計算に失敗しました。\n\n" +
							"未装備の御魂スロットにサブステータスが入力されています。";
				case CalculationInputValidationOutcome.SUB_STAT_TYPE_WHITHOUT_VALUE:
					return "ステータス計算に失敗しました。\n\n" +
							"サブステータスの種類が選択されていますが、値が入力されていません。";
				case CalculationInputValidationOutcome.SUB_STAT_VALUE_WHITHOUT_TYPE:
					return "ステータス計算に失敗しました。\n\n" +
							"サブステータスの値が入力されていますが、種類が選択されていません。";
				case CalculationInputValidationOutcome.INVALID_VALUE:
					return "ステータス計算に失敗しました。\n\n" +
							"数値欄に数値として扱えない文字が入力されています。";
				case CalculationInputValidationOutcome.NEGATIVE_VALUE:
					return "ステータス計算に失敗しました。\n\n" +
							"数値欄に負の値が入力されています。";
				case CalculationInputValidationOutcome.DUPLICATE_SUB_STAT:
					return "ステータス計算に失敗しました。\n\n" +
							"同じ御魂内に重複したサブステータスがあります。";
				case CalculationInputValidationOutcome.EFFECT_SLOT_COUNT_EXCEEDS_EQUIPPED_SLOTS:
					return "ステータス計算に失敗しました。\n\n" +
							"選択されている2セット効果と固有効果に対して、装備中の御魂数が足りません。";
				default:
					return "ステータス計算に失敗しました。\n\n" +
							"入力内容に不明なエラーがあります。";
			}
		}
	}
}
