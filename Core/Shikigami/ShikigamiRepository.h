#pragma once
#include "Shikigami.h"
#include <vector>

#include "../Outcome/ShikigamiDataOutcome.h"

class ShikigamiRepository
{
public:
	static ShikigamiDataOutcome getShikigamiList(const std::string& filePath, std::vector<Shikigami>& outShikigamis);
	static ShikigamiDataOutcome addShikigami(const std::string& filePath, const Shikigami& shikigami);
	static ShikigamiDataOutcome updateShikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData);
	static ShikigamiDataOutcome getRecoveryCandinateShikigamiList(const std::string& currentFilePath, const std::string& sourceFilePath, std::vector<Shikigami>& outRecoveryCandinateShikigamis);
	static ShikigamiDataOutcome recoverShikigamiList(const std::string& currentFilePath, const std::vector<Shikigami>& selectedShikigamis);
private:
	static bool isSameShikigami(const Shikigami& left, const Shikigami& right);
	static bool existsSameShikigami(const std::vector<Shikigami>& shikigamis, const Shikigami& target);
	static bool existsSameShikigamiExceptSelf(const std::vector<Shikigami>& shikigamis, const Shikigami& oldData, const Shikigami& newData);
	static int findInsertIndex(const std::vector<Shikigami>& shikigamis, const Shikigami& newData);
	static std::vector<Shikigami> extractUnknownShikigamiList(const std::vector<Shikigami>& currentShikigamis, const std::vector<Shikigami>& sourceShikigamis);
	static std::vector<Shikigami> mergeShikigamiList(const std::vector<Shikigami>& currentShikigamis, const std::vector<Shikigami>& sourceShikigamis);
};
