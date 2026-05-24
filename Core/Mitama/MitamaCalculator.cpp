#include "pch.h"
#include "MitamaCalculator.h"

Status MitamaCalculator::calculate(const MitamaSet& mitamaSet)
{
	Status totalStatus;

	totalStatus = calculateMitamas(mitamaSet.Mitamas);

	for (int i = 0; i < 3; i++) {
		applyStat(totalStatus, mitamaSet.SetEffects[i].Stat);
	}

	for (int i = 0; i < 6; i++) {
		applyStat(totalStatus, mitamaSet.UniqueEffects[i].Stat);
	}

	return totalStatus;
}

Status MitamaCalculator::calculateSingle(const Mitama& mitama)
{
	Status result;

	applyStat(result, mitama.MainStat);

	for (int i = 0; i < 4; i++) {
		applyStat(result, mitama.SubStat[i]);
	}

	return result;
}

Status MitamaCalculator::calculateMitamas(const Mitama mitamas[6])
{
	Status total;

	for (int i = 0; i < 6; i++) {
		Status one = MitamaCalculator::calculateSingle(mitamas[i]);

		total.Attack += one.Attack;
		total.Hp += one.Hp;
		total.Defense += one.Defense;
		total.Speed += one.Speed;

		total.CriticalRate += one.CriticalRate;
		total.CriticalDamage += one.CriticalDamage;
		total.EffectHit += one.EffectHit;
		total.EffectResist += one.EffectResist;

		total.AdditionalAttackRate += one.AdditionalAttackRate;
		total.AdditionalHpRate += one.AdditionalHpRate;
		total.AdditionalDefenseRate += one.AdditionalDefenseRate;
	}

	return total;
}

void MitamaCalculator::applyStat(Status& status, const StatValue& stat) {
	switch (stat.Type) {
		case StatType::Attack:
			status.Attack += stat.Value;
			break;
		case StatType::Hp:
			status.Hp += stat.Value;
			break;
		case StatType::Defense:
			status.Defense += stat.Value;
			break;
		case StatType::Speed:
			status.Speed += stat.Value;
			break;
		case StatType::CriticalRate:
			status.CriticalRate += stat.Value;
			break;
		case StatType::CriticalDamage:
			status.CriticalDamage += stat.Value;
			break;
		case StatType::EffectHit:
			status.EffectHit += stat.Value;
			break;
		case StatType::EffectResist:
			status.EffectResist += stat.Value;
			break;
		case StatType::AdditionalAttackRate:
			status.AdditionalAttackRate += stat.Value;
			break;
		case StatType::AdditionalHpRate:
			status.AdditionalHpRate += stat.Value;
			break;
		case StatType::AdditionalDefenseRate:
			status.AdditionalDefenseRate += stat.Value;
			break;
		default:
			break;
	}
}
