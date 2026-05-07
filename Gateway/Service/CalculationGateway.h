#pragma once

#include "../Dto/Model/StatusDto.h"
#include "../Dto/Model/MitamaSetDto.h"
#include "../Dto/Model/CalculationResultDto.h"

public ref class CalculationGateway
{
public:
	static CalculationResultDto^ calclutate(StatusDto^ baseStatusDto, MitamaSetDto^ mitamaSetDto);
};