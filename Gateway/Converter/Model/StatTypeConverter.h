#pragma once

#include "../../../Core/Model/StatType.h"
#include "../../Dto/Model/StatTypeDto.h"

// StatType と StatTypeDto の相互変換を行うクラス
class StatTypeConverter
{
public:
	static StatType toNative(StatTypeDto dto);
	static StatTypeDto toDto(StatType native);
};
