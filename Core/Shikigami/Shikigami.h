#pragma once
#include <string>

// 式神構造体
struct Shikigami
{
	std::string Rarity = "";
	std::string Name = "";

	double Attack = 0.0;
	double HP = 0.0;
	double Defense = 0.0;
	double Speed = 0.0;
	double CriticalRate = 0.0;
	double CriticalDamage = 0.0;
	double EffectHit = 0.0;
	double EffectResist = 0.0;
};
