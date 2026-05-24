#pragma once

#include "../../../Core/Outcome/ShikigamiDataOutcome.h"
#include "../../Dto/Outcome/ShikigamiDataOutcomeDto.h"

// ShikigamiDataOutcome から ShikigamiDataOutcomeDto への片方向変換を行うクラス
public ref class ShikigamiDataOutcomeMapper
{
public:
	static ShikigamiDataOutcomeDto toDto(ShikigamiDataOutcome outcome);
};
