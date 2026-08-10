#include "pch.h"
#include "StatusComparisonResultMapper.h"

StatusComparisonResultDto^ StatusComparisonResultMapper::toDto(const StatusComparisonResult& native)
{
	StatusComparisonResultDto^ dto = gcnew StatusComparisonResultDto();

	dto->AttackDifference = native.AttackDifference;
	dto->HpDifference = native.HpDifference;
	dto->DefenseDifference = native.DefenseDifference;
	dto->SpeedDifference = native.SpeedDifference;
	dto->CriticalRateDifference = native.CriticalRateDifference;
	dto->CriticalDamageDifference = native.CriticalDamageDifference;
	dto->EffectHitDifference = native.EffectHitDifference;
	dto->EffectResistDifference = native.EffectResistDifference;

	return dto;
}
