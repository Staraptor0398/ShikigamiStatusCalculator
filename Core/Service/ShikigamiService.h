#pragma once

#include <string>
#include <vector>

#include "../Outcome/ShikigamiDataOutcome.h"
#include "../Shikigami/Shikigami.h"

class ShikigamiService
{
public:
	static ShikigamiDataOutcome getShikigamiList(const std::string& filePath, std::vector<Shikigami>& shikigamiList);

	static ShikigamiDataOutcome addShikigami(const std::string& filePath, const Shikigami& shikigami);

	static ShikigamiDataOutcome updateShikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData);

	static ShikigamiDataOutcome getRecoveryCandinateShikigamiList(const std::string& currentFilePath, const std::string& sourceFilePath, std::vector<Shikigami>& recoveryCandinateShikigamiList);

	static ShikigamiDataOutcome recoverShikigamiList(const std::string& currentFilePath, const std::vector<Shikigami>& selectedShikigamiList);
};
