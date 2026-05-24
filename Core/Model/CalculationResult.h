#pragma once

#include "Status.h"

// 計算結果構造体
struct CalculationResult
{
	Status mitamaOnlyStatus;		// 御魂のみの加算結果
	Status finalStatus;				// 式神基礎値込みの最終計算結果
};
