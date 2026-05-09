#pragma once

#include "../../../Core/Mitama/Mitama.h"
#include "../../Dto/Model/MitamaDto.h"

class MitamaConverter
{
public:
	static Mitama toNative(MitamaDto^ dto);
	static MitamaDto^ toDto(const Mitama& native);
};
