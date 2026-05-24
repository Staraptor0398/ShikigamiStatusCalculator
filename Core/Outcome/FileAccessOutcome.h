#pragma once

// ファイルアクセス結果列挙体
//
// ファイル読み込み/書き込み処理結果を表す。
// ファイル自体に関する異常のみを扱う。
enum class FileAccessOutcome
{
	SUCCESS,				// 正常終了
	FILE_OPEN_FAILED,		// ファイルを開けなかった
	FILE_READ_FAILED,		// ファイル読み込み失敗
	FILE_WRITE_FAILED,		// ファイル書き込み失敗
	INVALID_FORMAT,			// CSV形式異常
	UNKNOWN_ERROR			// 不明なエラー
};
