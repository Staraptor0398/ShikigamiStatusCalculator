#pragma once
#include <vector>
#include "Shikigami.h"

#include "../Outcome/ShikigamiDataOutcome.h"

class ShikigamiRepository
{
public:
	static ShikigamiDataOutcome get_ShikigamiList(const std::string& filePath, std::vector<Shikigami>& outShikigamis);
	static ShikigamiDataOutcome add_Shikigami(const std::string& filePath, const Shikigami& shikigami);
	static ShikigamiDataOutcome update_Shikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData);
private:
	static bool isSameShikigami(const Shikigami& left, const Shikigami& right);
	static bool existsSameShikigami(const std::vector<Shikigami>& shikigamis, const Shikigami& target);
	static bool existsSameShikigamiExceptSelf(const std::vector<Shikigami>& shikigamis, const Shikigami& oldData, const Shikigami& newData);
	static int findInsertIndex(const std::vector<Shikigami>& shikigamis, const Shikigami& newData);
};