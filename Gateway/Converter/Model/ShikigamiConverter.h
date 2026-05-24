#pragma once

#include "../../../Core/Shikigami/Shikigami.h"
#include "../../Dto/Model/ShikigamiDto.h"

// Shikigami と ShikigamiDto の相互変換を行うクラス
class ShikigamiConverter
{
public:
	static Shikigami toNative(ShikigamiDto^ dto);
	static ShikigamiDto^ toDto(Shikigami native);
};
