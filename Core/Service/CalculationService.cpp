#include "pch.h"
#include "../Mitama/MitamaCalculator.h"
#include "../Status/StatusCalculator.h"
#include "CalculationService.h"

CalculationResult CalculationService::calculate(const Status& Status, const MitamaSet& mitamaSet)
{
	CalculationResult result;

	result.mitamaOnlyStatus = MitamaCalculator::calculate(mitamaSet);
	result.finalStatus = StatusCalculator::calculateFinalStatus(Status, result.mitamaOnlyStatus);

	return result;
}
