#pragma once

#include "StatusDto.h"

// 計算結果のDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の CalculationResult と対応するDto定義。
public ref class CalculationResultDto
{
public:
	property StatusDto^ MitamaOnlyStatus;		// 御魂のみの加算結果
	property StatusDto^ FinalStatus;			// 式神基礎値込みの最終計算結果
};
