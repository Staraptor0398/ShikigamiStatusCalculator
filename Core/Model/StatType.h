#pragma once

// ステータス種類定義
enum class StatType
{
	None,

	Attack,
	Hp,
	Defense,
	Speed,

	CriticalRate,
	CriticalDamage,
	EffectHit,
	EffectResist,

	AdditionalAttackRate,
	AdditionalHpRate,
	AdditionalDefenseRate,
};
