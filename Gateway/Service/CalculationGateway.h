#pragma once

#include "../Dto/StatusDto.h"
#include "../Dto/MitamaSetDto.h"
#include "../Dto/CalculationResultDto.h"

public ref class CalculationGateway
{
public:
	static CalculationResultDto^ calclutate(StatusDto^ baseStatusDto, MitamaSetDto^ mitamaSetDto);
};