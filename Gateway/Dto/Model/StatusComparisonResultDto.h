#pragma once

// // 計算結果スナップショット比較用の差分ステータスDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の StatusComparisonResult と対応するDto定義。
public ref class StatusComparisonResultDto {
public:
	property double AttackDifferense;
	property double HpDifferense;
	property double DefenseDifferense;
	property double SpeedDifferense;

	property double CriticalRateDifferense;
	property double CriticalDamageDifferense;
	property double EffectHitDifferense;
	property double EffectResistDifferense;
};
