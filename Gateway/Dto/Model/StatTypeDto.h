#pragma once

// ステータス種類Dto列挙体
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の StatType と対応するDto定義。
public enum class StatTypeDto
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
