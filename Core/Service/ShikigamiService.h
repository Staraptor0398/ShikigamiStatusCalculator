#pragma once

#include <string>
#include <vector>

#include "../Outcome/ShikigamiDataOutcome.h"
#include "../Shikigami/Shikigami.h"

class ShikigamiService
{
public:
	static ShikigamiDataOutcome get_ShikigamiList(const std::string& filePath, std::vector<Shikigami>& shikigamiList);

	static ShikigamiDataOutcome add_Shikigami(const std::string& filePath, const Shikigami& shikigami);

	static ShikigamiDataOutcome update_Shikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData);
};
