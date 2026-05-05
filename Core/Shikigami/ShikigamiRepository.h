#pragma once
#include <vector>
#include "Shikigami.h"

class ShikigamiRepository
{
public:
	static std::vector<Shikigami> get_ShikigamiList(const std::string& filePath);
	static void add_Shikigami(const std::string& filePath, const Shikigami& shikigami);
	static void update_Shikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData);
};