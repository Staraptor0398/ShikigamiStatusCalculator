#pragma once

#include "StatusDto.h"

public ref class CalculationResultDto
{
public:
	property StatusDto^ MitamaOnlyStatus;
	property StatusDto^ FinalStatus;
};