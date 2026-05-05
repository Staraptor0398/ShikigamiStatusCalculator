#pragma once
#include <string>

struct Shikigami
{
	std::string Rarity = "";
	std::string Name = "";

	double Attack = 0.0;
	double HP = 0.0;
	double Defense = 0.0;
	double Speed = 0.0;
	double CritRate = 0.0;
	double CritDamage = 0.0;
	double EffectHit = 0.0;
	double EffectResist = 0.0;
};