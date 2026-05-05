#pragma once
#include <vector>
#include <string>
#include "../Shikigami/Shikigami.h"

class FileAccess
{
public:
	static std::vector<Shikigami> load_Shikigami(const std::string& filePath);
	static void insert_Shikigami(const std::string& filePath, const Shikigami& newData);
};