#pragma once

// 式紙データ捜査結果Dto列挙体
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の ShikigamiDataOutcome と対応するDto定義。
public enum class ShikigamiDataOutcomeDto
{
	SUCCESS,				// 正常終了
	FILE_OPEN_FAILED,		// ファイルを開けなかった
	FILE_READ_FAILED,		// ファイル読み込み失敗
	FILE_WRITE_FAILED,		// ファイル書き込み失敗
	INVALID_FORMAT,			// CSV形式異常
	NOT_FOUND,				// 対象式神データが存在しない
	DUPLICATE,				// 同一式神データが既に存在する
	UNKNOWN_ERROR			// 不明なエラー
};
