#pragma once

// ステータス計算で使用する値を保持するDtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の Status と対応するDto定義。
public ref class StatusDto
{
public:
	property double Attack;
	property double HP;
	property double Defense;
	property double Speed;

	property double CritRate;
	property double CritDamage;
	property double EffectHit;
	property double EffectResist;

	property double AdditionalAttackRate;
	property double AdditionalHpRate;
	property double AdditionalDefenseRate;
};
