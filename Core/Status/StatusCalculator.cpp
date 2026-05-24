#include "pch.h"
#include "StatusCalculator.h"

Status StatusCalculator::calculateFinalStatus(const Status& baseStatus, const Status& mitamaStatus)
{
	Status result;

	result.Attack = baseStatus.Attack + mitamaStatus.Attack + (baseStatus.Attack * mitamaStatus.AdditionalAttackRate / 100.0);
	result.Hp = baseStatus.Hp + mitamaStatus.Hp + (baseStatus.Hp * mitamaStatus.AdditionalHpRate / 100.0);
	result.Defense = baseStatus.Defense + mitamaStatus.Defense + (baseStatus.Defense * mitamaStatus.AdditionalDefenseRate / 100.0);
	result.Speed = baseStatus.Speed + mitamaStatus.Speed;
	result.CriticalRate = baseStatus.CriticalRate + mitamaStatus.CriticalRate;
	result.CriticalDamage = baseStatus.CriticalDamage + mitamaStatus.CriticalDamage;
	result.EffectHit = baseStatus.EffectHit + mitamaStatus.EffectHit;
	result.EffectResist = baseStatus.EffectResist + mitamaStatus.EffectResist;

	return result;
}
