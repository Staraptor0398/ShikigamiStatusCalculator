#pragma once

#include "StatTypeDto.h"

// ステータス種類と値を保持するDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の StatValue と対応するDto定義。
public ref class StatValueDto
{
public:
	StatTypeDto Type = StatTypeDto::None;
	double Value = 0.0;
};
