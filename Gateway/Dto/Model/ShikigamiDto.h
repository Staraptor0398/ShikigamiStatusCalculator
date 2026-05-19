#pragma once

using namespace System;

public ref class ShikigamiDto
{
public:
	String^ Name;
	String^ Rarity;

	double Attack;
	double HP;
	double Defense;
	double Speed;
	double CritRate;
	double CritDamage;
	double EffectHit;
	double EffectResist;

	virtual System::String^ ToString() override {
		return Name;
	}
};
