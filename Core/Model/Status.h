#pragma once

// ステータス計算で使用する値を保持する構造体
struct Status {
	double Attack = 0.0;
	double Hp = 0.0;
	double Defense = 0.0;
	double Speed = 0.0;

	double CriticalRate = 0.0;
	double CriticalDamage = 0.0;
	double EffectHit = 0.0;
	double EffectResist = 0.0;

	double AdditionalAttackRate = 0.0;
	double AdditionalHpRate = 0.0;
	double AdditionalDefenseRate = 0.0;
};
