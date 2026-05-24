#pragma once

#include "StatValueDto.h"

// // 2セット効果・固有効果用ステータスDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の SetEffect と対応するDto定義。
public ref class SetEffectDto
{
public:
	property StatValueDto^ Stat;
};
