#pragma once

#include "../Mitama/MitamaSet.h"
#include "../Model/CalculationResult.h"
#include "../Model/Status.h"

class CalculationService
{
public:
	static CalculationResult calculate(const Status& baseStatus, const MitamaSet& mitamaSet);
};
