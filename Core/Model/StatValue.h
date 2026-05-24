#pragma once

#include "StatType.h"

// ステータス種類と値を保持する構造体
struct StatValue
{
	StatType Type = StatType::None;
	double Value = 0.0;
};
