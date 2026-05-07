#pragma once

#include "StatTypeDto.h"

public ref class StatValueDto
{
public:
	StatTypeDto Type = StatTypeDto::None;
	double Value = 0.0;
};