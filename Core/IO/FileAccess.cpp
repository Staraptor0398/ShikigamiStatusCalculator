#include "pch.h"
#include "FileAccess.h"

#include <fstream>
#include <sstream>

namespace {
	std::vector<std::string> spilit_CsvLine(const std::string& line) {
		std::vector<std::string> values;
		std::stringstream stream(line);
		std::string value;

		while (std::getline(stream, value, ',')) {
			values.push_back(value);
		}

		return values;
	}

	double to_Double(const std::string& text) {
		if (text.empty()) {
			return 0.0;
		}

		return std::stod(text);
	}
}

std::vector<Shikigami> FileAccess::load_Shikigami(const std::string& filePath)
{
	std::vector<Shikigami> shikigamiList;

	std::ifstream file(filePath);

	if (!file.is_open()) {
		return shikigamiList;
	}

	std::string line;

	std::getline(file, line);

	while (std::getline(file, line)) {
		if (line.empty()) {
			continue;
		}

		std::vector<std::string> values = spilit_CsvLine(line);

		if (values.size() < 10) {
			continue;
		}

		Shikigami shikigami;

		shikigami.Rarity = values[0];
		shikigami.Name = values[1];
		shikigami.Attack = to_Double(values[2]);
		shikigami.HP = to_Double(values[3]);
		shikigami.Defense = to_Double(values[4]);
		shikigami.Speed = to_Double(values[5]);
		shikigami.CritRate = to_Double(values[6]);
		shikigami.CritDamage = to_Double(values[7]);
		shikigami.EffectHit = to_Double(values[8]);
		shikigami.EffectResist = to_Double(values[9]);

		shikigamiList.push_back(shikigami);
	}

	return shikigamiList;
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

	std::ofstream file(filePath, std::ios::trunc);

	if (!file.is_open()) {
		return;
	}

	for (const auto& s : list) {
		file
			<< s.Rarity << ","
			<< s.Name << ","
			<< s.Attack << ","
			<< s.HP << ","
			<< s.Defense << ","
			<< s.Speed << ","
			<< s.CritRate << ","
			<< s.CritDamage << ","
			<< s.EffectHit << ","
			<< s.EffectResist
			<< std::endl;
	}
}