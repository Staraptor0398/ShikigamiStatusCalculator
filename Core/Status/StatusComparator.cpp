#include "pch.h"
#include "StatusComparator.h"

StatusComparisonResult StatusComparator::compare(const Status& baseStatus, const Status& targetStatus)
{
	StatusComparisonResult result;

	result.AttackDifferense = targetStatus.Attack - baseStatus.Attack;
	result.HpDifferense = targetStatus.Hp - baseStatus.Hp;
	result.DefenseDifferense = targetStatus.Defense - baseStatus.Defense;
	result.SpeedDifferense = targetStatus.Speed - baseStatus.Speed;
	result.CriticalRateDifferense = targetStatus.CriticalRate - baseStatus.CriticalRate;
	result.CriticalDamageDifferense = targetStatus.CriticalDamage - baseStatus.CriticalDamage;
	result.EffectHitDifferense = targetStatus.EffectHit - baseStatus.EffectHit;
	result.EffectResistDifferense = targetStatus.EffectResist - baseStatus.EffectResist;

	return result;
}
