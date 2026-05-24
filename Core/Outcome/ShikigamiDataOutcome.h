#pragma once

// 式神データ操作結果列挙体
//
// ShikigamiRepository の処理結果を表す。
// ファイルアクセス異常に加えて、
// 式神データとしての整合性異常も扱う。
enum class ShikigamiDataOutcome
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
