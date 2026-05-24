#include "pch.h"
#include "ShikigamiService.h"

#include "../Shikigami/ShikigamiRepository.h"

ShikigamiDataOutcome ShikigamiService::getShikigamiList(const std::string& filePath, std::vector<Shikigami>& shikigamiList)
{
	return ShikigamiRepository::getShikigamiList(filePath, shikigamiList);
}

ShikigamiDataOutcome ShikigamiService::addShikigami(const std::string& filePath, const Shikigami& shikigami)
{
	return ShikigamiRepository::addShikigami(filePath, shikigami);
}

ShikigamiDataOutcome ShikigamiService::updateShikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData)
{
	return ShikigamiRepository::updateShikigami(filePath, oldData, newData);
}
