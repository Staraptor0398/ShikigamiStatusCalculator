#pragma once

#include "../../../Core/Model/StatValue.h"
#include "../../Dto/Model/StatValueDto.h"

// StatValue と StatValueDto の相互変換を行うクラス
class StatValueConverter
{
public:
	static StatValue toNative(StatValueDto^ dto);
	static StatValueDto^ toDto(const StatValue& native);
};
