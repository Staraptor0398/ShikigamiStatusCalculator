#include "pch.h"
#include "StatusComparisonResultMapper.h"

StatusComparisonResultDto^ StatusComparisonResultMapper::toDto(const StatusComparisonResult& native)
{
	StatusComparisonResultDto^ dto = gcnew StatusComparisonResultDto();

	dto->AttackDifferense = native.AttackDifferense;
	dto->HpDifferense = native.HpDifferense;
	dto->DefenseDifferense = native.DefenseDifferense;
	dto->SpeedDifferense = native.SpeedDifferense;
	dto->CriticalRateDifferense = native.CriticalRateDifferense;
	dto->CriticalDamageDifferense = native.CriticalDamageDifferense;
	dto->EffectHitDifferense = native.EffectHitDifferense;
	dto->EffectResistDifferense = native.EffectResistDifferense;

	return dto;
}
