#pragma once

#include "../../../Core/Outcome/ShikigamiDataOutcome.h"
#include "../../Dto/Outcome/ShikigamiDataOutcomeDto.h"

public ref class ShikigamiDataOutcomeMapper
{
public:
	static ShikigamiDataOutcomeDto toDto(ShikigamiDataOutcome outcome);
};