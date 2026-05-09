#pragma once

#include <string>
#include <vector>

#include "../../Shikigami/Shikigami.h"

class ShikigamiCsvConverter
{
public:
	static Shikigami toShikigami(const std::string& csvLine);
	static std::string toCsvLine(const Shikigami& shikigami);

private:
	static std::vector<std::string> splitCsvColumns(const std::string& csvLine);
	static std::string joinCsvLineColumns(const std::vector<std::string>& csvLineColumns);

	static Shikigami toShikigami(const std::vector<std::string>& csvLineColumns);
	static std::vector<std::string> toCsvLineColumns(const Shikigami& shikigami);
};

