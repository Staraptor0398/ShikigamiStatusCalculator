#pragma once

#include "../../../Core/Mitama/Mitama.h"
#include "../../Dto/Model/MitamaDto.h"

// Mitama と MitamaDto の相互変換を行うクラス
class MitamaConverter
{
public:
	static Mitama toNative(MitamaDto^ dto);
	static MitamaDto^ toDto(const Mitama& native);
};
