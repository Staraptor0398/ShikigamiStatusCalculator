#pragma once

#include "Mitama.h"
#include "SetEffect.h"

// 御魂セットの構造体
struct MitamaSet
{
	Mitama Mitamas[6];				// 御魂6スロット分
	SetEffect SetEffects[3];		// 2セット効果3個分
	SetEffect UniqueEffects[6];		// 固有効果6個分
};
