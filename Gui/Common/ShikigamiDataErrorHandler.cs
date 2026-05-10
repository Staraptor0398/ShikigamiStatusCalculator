using System.Windows.Forms;

namespace Gui.Common
{
	public static class ShikigamiDataErrorHandler
	{
		public static bool Handle(ShikigamiDataOutcomeDto outcome, string operationName)
		{
			if (outcome == ShikigamiDataOutcomeDto.SUCCESS)
			{
				return false;
			}

			string message = createMessage(outcome, operationName);
			string logMessage = message.Replace("\n\n", " ");

			Logger.Error($"Operation={operationName} Outcome={outcome} Message={logMessage}");

			MessageBox.Show(
				message,
				operationName,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);

			return true;
		}

		private static string createMessage(ShikigamiDataOutcomeDto outcome, string operationName)
		{
			switch (outcome)
			{
				case ShikigamiDataOutcomeDto.FILE_OPEN_FAILED:
					return $"{operationName}に失敗しました。\n\n" +
							"式神データファイルを開けませんでした。";
				case ShikigamiDataOutcomeDto.FILE_READ_FAILED:
					return $"{operationName}に失敗しました。\n\n" +
							"式神データファイルの読み込み中にエラーが発生しました。";
				case ShikigamiDataOutcomeDto.FILE_WRITE_FAILED:
					return $"{operationName}に失敗しました。\n\n" +
							"式神データファイルの書き込みに失敗しました。";
				case ShikigamiDataOutcomeDto.INVALID_FORMAT:
					return $"{operationName}に失敗しました。\n\n" +
							"式神データファイルの形式が正しくありません。";
				case ShikigamiDataOutcomeDto.NOT_FOUND:
					return $"{operationName}に失敗しました。\n\n" +
							"対象の式神データが見つかりませんでした。";
				case ShikigamiDataOutcomeDto.DUPLICATE:
					return $"{operationName}に失敗しました。\n\n" +
							"同じレアリティ・同じ式神名のデータが既に存在します。";
				case ShikigamiDataOutcomeDto.UNKNOWN_ERROR:
				default:
					return $"{operationName}に失敗しました。\n\n" +
							"不明なエラーが発生しました。";
			}
		}
	}
}
