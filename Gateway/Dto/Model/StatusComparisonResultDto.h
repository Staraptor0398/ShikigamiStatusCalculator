#pragma once

// // 計算結果スナップショット比較用の差分ステータスDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の StatusComparisonResult と対応するDto定義。
public ref class StatusComparisonResultDto {
public:
	property double AttackDifference;
	property double HpDifference;
	property double DefenseDifference;
	property double SpeedDifference;

	property double CriticalRateDifference;
	property double CriticalDamageDifference;
	property double EffectHitDifference;
	property double EffectResistDifference;
};
