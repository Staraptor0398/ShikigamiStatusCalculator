#pragma once

#include "../../../Core/Mitama/MitamaSet.h"
#include "../../Dto/Model/MitamaSetDto.h"

// MitamaSet と MitamaSetDto の相互変換を行うクラス
class MitamaSetConverter
{
public:
	static MitamaSet toNative(MitamaSetDto^ dto);
	static MitamaSetDto^ toDto(const MitamaSet& native);
};
