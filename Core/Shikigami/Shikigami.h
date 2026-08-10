#pragma once
#include <string>

#include "../Model/Status.h"
#include "ShikigamiRarity.h"

// 式神構造体
struct Shikigami
{
	ShikigamiRarity Rarity;
	std::string Name = "";

	Status Status;
};
