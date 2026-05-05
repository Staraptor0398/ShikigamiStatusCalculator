#pragma once

#include "../Model/Status.h"
#include "../Model/CalculationResult.h"
#include "../Mitama/MitamaSet.h"

class CalculationService
{
public:
	static CalculationResult calculate(const Status& baseStatus, const MitamaSet& mitamaSet);
};