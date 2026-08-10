#pragma once

// 計算結果スナップショット比較用の差分ステータス
struct StatusComparisonResult
{
	double AttackDifference = 0.0;
	double HpDifference = 0.0;
	double DefenseDifference = 0.0;
	double SpeedDifference = 0.0;

	double CriticalRateDifference = 0.0;
	double CriticalDamageDifference = 0.0;
	double EffectHitDifference = 0.0;
	double EffectResistDifference = 0.0;
};
