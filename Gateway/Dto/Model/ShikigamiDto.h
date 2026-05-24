#pragma once

using namespace System;

// 式神Dtoクラス
//
// Gui ⇔ Gateway 間のデータ受け渡しで使用する。
// Core の Shikigami と対応するDto定義。
public ref class ShikigamiDto
{
public:
	String^ Name;
	String^ Rarity;

	double Attack;
	double HP;
	double Defense;
	double Speed;
	double CriticalRate;
	double CriticalDamage;
	double EffectHit;
	double EffectResist;

	virtual System::String^ ToString() override {
		return Name;
	}
};
