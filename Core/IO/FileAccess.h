#pragma once
#include <vector>
#include <string>
#include "../Shikigami/Shikigami.h"

#include "../Outcome/FileAccessOutcome.h"

class FileAccess
{
public:
	static FileAccessOutcome load_Shikigami(const std::string& filePath, std::vector<Shikigami>& outShikigamis);
	static FileAccessOutcome save_Shikigami(const std::string& filePath, const std::vector<Shikigami> shikigamis);
};