#pragma once

#include "../../Core/Shikigami/Shikigami.h"
#include "../Dto/ShikigamiDto.h"

class ShikigamiConverter
{
public:
	static Shikigami toNative(ShikigamiDto^ dto);
	static ShikigamiDto^ toDto(Shikigami native);
};