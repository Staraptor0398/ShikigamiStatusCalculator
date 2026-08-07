#pragma once

#include<vector>
using namespace System::Collections::Generic;

#include "../../../Core/Shikigami/Shikigami.h"
#include "../../Dto/Model/ShikigamiDto.h"

// std::vector<Shikigami> と List<ShikigamiDto^>^ の相互変換を行うクラス
class ShikigamiListConverter
{
public:
	static std::vector<Shikigami> toNative(List<ShikigamiDto^>^ dto);
	static List<ShikigamiDto^>^ toDto(std::vector<Shikigami> native);
};
