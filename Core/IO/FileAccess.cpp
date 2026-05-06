#include "pch.h"
#include "FileAccess.h"

#include <fstream>
#include "../Converter/ShikigamiCsvConverter.h"

namespace {
	const std::string SHIKIGAMI_CSV_HEADER = "レア度,式神名,攻撃力,HP,防御力,素早さ,会心率,会心DMG,効果命中,効果抵抗";
}

std::vector<Shikigami> FileAccess::load_Shikigami(const std::string& filePath)
{
	std::vector<Shikigami> shikigamiList;

	std::ifstream file(filePath);

	if (!file.is_open()) {
		return shikigamiList;
	}

	std::string line;

	// ヘッダー行を読み飛ばす
	std::getline(file, line);

	while (std::getline(file, line)) {
		if (line.empty()) {
			continue;
		}

		shikigamiList.push_back(ShikigamiCsvConverter::toShikigami(line));
	}

	return shikigamiList;
}

void FileAccess::save_Shikigami(const std::string& filePath, const std::vector<Shikigami> shikigamis)
{
	std::ofstream file(filePath, std::ios::trunc);

	if (!file.is_open()) {
		return;
	}

	// ヘッダー行を書き戻す
	file << SHIKIGAMI_CSV_HEADER << std::endl;

	for (const auto& s : shikigamis) {
		file << ShikigamiCsvConverter::toCsvLine(s) << std::endl;
	}
}

void FileAccess::insert_Shikigami(const std::string& filePath, const Shikigami& newData)
{
	auto list = load_Shikigami(filePath);

	int insertIndex = list.size();

	for (int i = 0; i < list.size(); i++) {
		if (list[i].Rarity == newData.Rarity) {
			insertIndex = i + 1;
		}
	}

	list.insert(list.begin() + insertIndex, newData);

	save_Shikigami(filePath, list);
}

void FileAccess::update_Shikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData)
{
	auto list = load_Shikigami(filePath);

	bool isUpdated = false;

	for (auto& s : list) {
		if (s.Rarity == oldData.Rarity && s.Name == oldData.Name) {
			s = newData;
			isUpdated = true;
			break;
		}
	}

	if (!isUpdated) {
		return;
	}

	save_Shikigami(filePath, list);
}