#include "pch.h"
#include "MitamaCalculatorTestData.h"

MitamaSet MitamaCalculatorTestData::createValidMitamaSetTestData()
{
	MitamaSet mitamaSet{};
	// ========================================
	// MainStat
	// ========================================
	mitamaSet.Mitamas[0].MainStat.Type = StatType::Attack;
	mitamaSet.Mitamas[0].MainStat.Value = 486.0;
	mitamaSet.Mitamas[1].MainStat.Type = StatType::Speed;
	mitamaSet.Mitamas[1].MainStat.Value = 57.0;
	mitamaSet.Mitamas[2].MainStat.Type = StatType::Defense;
	mitamaSet.Mitamas[2].MainStat.Value = 104.0;
	mitamaSet.Mitamas[3].MainStat.Type = StatType::EffectHit;
	mitamaSet.Mitamas[3].MainStat.Value = 55.0;
	mitamaSet.Mitamas[4].MainStat.Type = StatType::Hp;
	mitamaSet.Mitamas[4].MainStat.Value = 2052.0;
	mitamaSet.Mitamas[5].MainStat.Type = StatType::CriticalDamage;
	mitamaSet.Mitamas[5].MainStat.Value = 89.0;

	// ========================================
	// SubStat
	// ========================================

	// Mitama 1
	mitamaSet.Mitamas[0].SubStat[0] = { StatType::AdditionalAttackRate, 10.0 };
	mitamaSet.Mitamas[0].SubStat[1] = { StatType::CriticalRate, 5.0 };
	mitamaSet.Mitamas[0].SubStat[2] = { StatType::Speed, 6.0 };
	mitamaSet.Mitamas[0].SubStat[3] = { StatType::EffectResist, 8.0 };

	// Mitama 2
	mitamaSet.Mitamas[1].SubStat[0] = { StatType::AdditionalHpRate, 12.0 };
	mitamaSet.Mitamas[1].SubStat[1] = { StatType::CriticalDamage, 7.0 };
	mitamaSet.Mitamas[1].SubStat[2] = { StatType::Attack, 20.0 };
	mitamaSet.Mitamas[1].SubStat[3] = { StatType::EffectHit, 9.0 };

	// Mitama 3
	mitamaSet.Mitamas[2].SubStat[0] = { StatType::AdditionalDefenseRate, 15.0 };
	mitamaSet.Mitamas[2].SubStat[1] = { StatType::Defense, 18.0 };
	mitamaSet.Mitamas[2].SubStat[2] = { StatType::Hp, 120.0 };
	mitamaSet.Mitamas[2].SubStat[3] = { StatType::CriticalRate, 4.0 };

	// Mitama 4
	mitamaSet.Mitamas[3].SubStat[0] = { StatType::Attack, 25.0 };
	mitamaSet.Mitamas[3].SubStat[1] = { StatType::Hp, 150.0 };
	mitamaSet.Mitamas[3].SubStat[2] = { StatType::Defense, 12.0 };
	mitamaSet.Mitamas[3].SubStat[3] = { StatType::Speed, 5.0 };

	// Mitama 5
	mitamaSet.Mitamas[4].SubStat[0] = { StatType::CriticalDamage, 11.0 };
	mitamaSet.Mitamas[4].SubStat[1] = { StatType::EffectHit, 6.0 };
	mitamaSet.Mitamas[4].SubStat[2] = { StatType::EffectResist, 7.0 };
	mitamaSet.Mitamas[4].SubStat[3] = { StatType::Attack, 22.0 };

	// Mitama 6
	mitamaSet.Mitamas[5].SubStat[0] = { StatType::Hp, 180.0 };
	mitamaSet.Mitamas[5].SubStat[1] = { StatType::Defense, 16.0 };
	mitamaSet.Mitamas[5].SubStat[2] = { StatType::CriticalRate, 3.0 };
	mitamaSet.Mitamas[5].SubStat[3] = { StatType::Speed, 4.0 };

	// ========================================
	// SetEffect
	// 2個 = 4枠消費
	// ========================================
	mitamaSet.SetEffects[0] = { StatType::AdditionalAttackRate, 15.0 };
	mitamaSet.SetEffects[1] = { StatType::AdditionalHpRate, 15.0 };

	// SetEffects[2] は未設定

	// ========================================
	// UniqueEffect
	// 2個 = 2枠消費
	// 合計6枠
	// ========================================
	mitamaSet.UniqueEffects[0] = { StatType::CriticalRate, 8.0 };
	mitamaSet.UniqueEffects[1] = { StatType::EffectHit, 8.0 };

	// UniqueEffects[2〜5] は未設定

	return mitamaSet;
}
