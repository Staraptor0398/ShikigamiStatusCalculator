#include "pch.h"
#include "FileAccess.h"

#include <fstream>
#include "../Converter/Format/ShikigamiCsvConverter.h"

namespace {
	const std::string SHIKIGAMI_CSV_HEADER = "レア度,式神名,攻撃力,HP,防御力,素早さ,会心率,会心DMG,効果命中,効果抵抗";
}

FileAccessOutcome FileAccess::loadShikigami(const std::string& filePath, std::vector<Shikigami>& outShikigamis)
{
	outShikigamis.clear();

	std::ifstream file(filePath);

	if (!file.is_open()) {
		return FileAccessOutcome::FILE_OPEN_FAILED;
	}

	std::string headerLine;

	// ヘッダー行を読み飛ばす
	if (!std::getline(file, headerLine)) {
		return FileAccessOutcome::INVALID_FORMAT;
	}

	if (headerLine != SHIKIGAMI_CSV_HEADER) {
		return FileAccessOutcome::INVALID_FORMAT;
	}

	std::string line;

	while (std::getline(file, line)) {
		if (line.empty()) {
			continue;
		}

		outShikigamis.push_back(ShikigamiCsvConverter::toShikigami(line));
	}

	if (file.bad()) {
		return FileAccessOutcome::FILE_READ_FAILED;
	}

	return FileAccessOutcome::SUCCESS;
}

FileAccessOutcome FileAccess::saveShikigami(const std::string& filePath, const std::vector<Shikigami> shikigamis)
{
	std::ofstream file(filePath, std::ios::trunc);

	if (!file.is_open()) {
		return FileAccessOutcome::FILE_OPEN_FAILED;
	}

	// ヘッダー行を書き戻す
	file << SHIKIGAMI_CSV_HEADER << std::endl;

	for (const auto& s : shikigamis) {
		file << ShikigamiCsvConverter::toCsvLine(s) << std::endl;
	}

	if (!file) {
		return FileAccessOutcome::FILE_WRITE_FAILED;
	}

	return FileAccessOutcome::SUCCESS;
}
