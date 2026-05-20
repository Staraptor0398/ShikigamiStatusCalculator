#include "pch.h"
#include "ShikigamiService.h"

#include "../Shikigami/ShikigamiRepository.h"

ShikigamiDataOutcome ShikigamiService::get_ShikigamiList(const std::string& filePath, std::vector<Shikigami>& shikigamiList)
{
	return ShikigamiRepository::get_ShikigamiList(filePath, shikigamiList);
}

ShikigamiDataOutcome ShikigamiService::add_Shikigami(const std::string& filePath, const Shikigami& shikigami)
{
	return ShikigamiRepository::add_Shikigami(filePath, shikigami);
}

ShikigamiDataOutcome ShikigamiService::update_Shikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData)
{
	return ShikigamiRepository::update_Shikigami(filePath, oldData, newData);
}
