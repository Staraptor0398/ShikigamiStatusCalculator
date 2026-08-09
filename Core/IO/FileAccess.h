#pragma once
#include "../Shikigami/Shikigami.h"
#include <string>
#include <vector>

#include "../Outcome/FileAccessOutcome.h"

class FileAccess
{
public:
	static FileAccessOutcome loadShikigami(const std::string& filePath, std::vector<Shikigami>& outShikigamis);
	static FileAccessOutcome loadValidShikigami(const std::string& filePath, std::vector<Shikigami>& outShikigamis);
	static FileAccessOutcome saveShikigami(const std::string& filePath, const std::vector<Shikigami> shikigamis);
};
