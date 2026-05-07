#pragma once

#include "../../../Core/Shikigami/Shikigami.h"
#include "../../Dto/Model/ShikigamiDto.h"

class ShikigamiConverter
{
public:
	static Shikigami toNative(ShikigamiDto^ dto);
	static ShikigamiDto^ toDto(Shikigami native);
};