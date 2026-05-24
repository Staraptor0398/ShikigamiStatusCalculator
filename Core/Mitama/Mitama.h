#pragma once

#include "../Model/StatValue.h"

// 御魂1個分の構造体
struct Mitama
{
	StatValue MainStat;				// メインステータス
	StatValue SubStat[4];			// サブステータス4個分
};
