#include "pch.h"
#include "StatusComparator.h"

StatusComparisonResult StatusComparator::compare(const Status& baseStatus, const Status& targetStatus)
{
	StatusComparisonResult result;

	result.AttackDifference = targetStatus.Attack - baseStatus.Attack;
	result.HpDifference = targetStatus.Hp - baseStatus.Hp;
	result.DefenseDifference = targetStatus.Defense - baseStatus.Defense;
	result.SpeedDifference = targetStatus.Speed - baseStatus.Speed;
	result.CriticalRateDifference = targetStatus.CriticalRate - baseStatus.CriticalRate;
	result.CriticalDamageDifference = targetStatus.CriticalDamage - baseStatus.CriticalDamage;
	result.EffectHitDifference = targetStatus.EffectHit - baseStatus.EffectHit;
	result.EffectResistDifference = targetStatus.EffectResist - baseStatus.EffectResist;

	return result;
}
