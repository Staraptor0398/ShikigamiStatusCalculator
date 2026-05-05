#include "pch.h"
#include "ShikigamiRepository.h"
#include "../IO/FileAccess.h"

std::vector<Shikigami> ShikigamiRepository::get_ShikigamiList(const std::string& filePath)
{
	return FileAccess::load_Shikigami(filePath);
}

void ShikigamiRepository::add_Shikigami(const std::string& filePath, const Shikigami& shikigami)
{
	FileAccess::insert_Shikigami(filePath, shikigami);
}