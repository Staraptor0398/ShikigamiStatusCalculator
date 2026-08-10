#include "pch.h"
#include "ShikigamiCsvConverter.h"

#include <sstream>

namespace {
	enum CsvColumnIndex {
		RARITY_INDEX,
		NAME_INDEX,
		ATTACK_INDEX,
		HP_INDEX,
		DEFENSE_INDEX,
		SPEED_INDEX,
		CRIT_RATE_INDEX,
		CRIT_DAMAGE_INDEX,
		EFFECT_HIT_INDEX,
		EFFECT_RESIST_INDEX,

		SHIKIGAMI_CSV_COLUMNCOUNT
	};
}

Shikigami ShikigamiCsvConverter::toShikigami(const std::string& csvLine)
{
	std::vector<std::string> csvLineColumns = splitCsvColumns(csvLine);

	return toShikigami(csvLineColumns);
}

std::string ShikigamiCsvConverter::toCsvLine(const Shikigami& shikigami)
{
	std::vector<std::string> csvLineColumns = toCsvLineColumns(shikigami);

	return joinCsvLineColumns(csvLineColumns);
}

std::vector<std::string> ShikigamiCsvConverter::splitCsvColumns(const std::string& csvLine)
{
	std::vector<std::string> csvLineColumns;
	std::stringstream stream(csvLine);
	std::string column;

	while (std::getline(stream, column, ',')) {
		csvLineColumns.push_back(column);
	}

	return csvLineColumns;
}

std::string ShikigamiCsvConverter::joinCsvLineColumns(const std::vector<std::string>& csvLineColumns)
{
	std::string csvLine;

	for (int i = 0; i < static_cast<int>(csvLineColumns.size()); i++) {
		if (i != 0) {
			csvLine += ",";
		}

		csvLine += csvLineColumns[i];
	}

	return csvLine;
}

Shikigami ShikigamiCsvConverter::toShikigami(const std::vector<std::string>& csvLineColumns)
{
	if (csvLineColumns.size() != SHIKIGAMI_CSV_COLUMNCOUNT) {
		throw std::invalid_argument("Invalid shikigami CSV culumn count.");
	}

	Shikigami shikigami;

	shikigami.Rarity = csvLineColumns[RARITY_INDEX];
	shikigami.Name = csvLineColumns[NAME_INDEX];
	shikigami.Status.Attack = std::stod(csvLineColumns[ATTACK_INDEX]);
	shikigami.Status.Hp = std::stod(csvLineColumns[HP_INDEX]);
	shikigami.Status.Defense = std::stod(csvLineColumns[DEFENSE_INDEX]);
	shikigami.Status.Speed = std::stod(csvLineColumns[SPEED_INDEX]);
	shikigami.Status.CriticalRate = std::stod(csvLineColumns[CRIT_RATE_INDEX]);
	shikigami.Status.CriticalDamage = std::stod(csvLineColumns[CRIT_DAMAGE_INDEX]);
	shikigami.Status.EffectHit = std::stod(csvLineColumns[EFFECT_HIT_INDEX]);
	shikigami.Status.EffectResist = std::stod(csvLineColumns[EFFECT_RESIST_INDEX]);

	return shikigami;
}

std::vector<std::string> ShikigamiCsvConverter::toCsvLineColumns(const Shikigami& shikigami)
{
	std::vector<std::string> csvLineColumns;

	csvLineColumns.push_back(shikigami.Rarity.toString());
	csvLineColumns.push_back(shikigami.Name);
	csvLineColumns.push_back(std::to_string(shikigami.Status.Attack));
	csvLineColumns.push_back(std::to_string(shikigami.Status.Hp));
	csvLineColumns.push_back(std::to_string(shikigami.Status.Defense));
	csvLineColumns.push_back(std::to_string(shikigami.Status.Speed));
	csvLineColumns.push_back(std::to_string(shikigami.Status.CriticalRate));
	csvLineColumns.push_back(std::to_string(shikigami.Status.CriticalDamage));
	csvLineColumns.push_back(std::to_string(shikigami.Status.EffectHit));
	csvLineColumns.push_back(std::to_string(shikigami.Status.EffectResist));

	return csvLineColumns;
}
