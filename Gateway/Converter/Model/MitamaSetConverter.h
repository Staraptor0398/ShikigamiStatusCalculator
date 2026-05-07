#pragma once

#include "../../../Core/Mitama/MitamaSet.h"
#include "../../Dto/Model/MitamaSetDto.h"

class MitamaSetConverter
{
public:
	static MitamaSet toNative(MitamaSetDto^ dto);
	static MitamaSetDto^ toDto(const MitamaSet& native);
};