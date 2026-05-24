#pragma once

// 計算結果スナップショット比較用の差分ステータス
struct StatusComparisonResult
{
	double AttackDifferense = 0.0;
	double HpDifferense = 0.0;
	double DefenseDifferense = 0.0;
	double SpeedDifferense = 0.0;

	double CriticalRateDifferense = 0.0;
	double CriticalDamageDifferense = 0.0;
	double EffectHitDifferense = 0.0;
	double EffectResistDifferense = 0.0;
};
