#include "pch.h"
#include "ShikigamiCsvConverter.h"

#include <sstream>

namespace {
	enum CsvColumnIndex {
		RARITY_INDEX,
		NAME_INDEX,
		ATTACK_INDEX,
		HP_INDEX,
		DEFFENSE_INDEX,
		SPEED_INDEX,
		CRIT_RATE_INDEX,
		CRIT_DAMAGE_INDEX,
		EFFECT_HIT_INDEX,
		EFFECT_RESITST_INDEX,

		SHIKIGAMI_CSV_COLUMNCOUNT
	};
}

Shikigami ShikigamiCsvConverter::toShikigami(const std::string& csvLine)
{
	std::vector<std::string> csvLineColumns = spilitCsvColumns(csvLine);

	return toShikigami(csvLineColumns);
}

std::string ShikigamiCsvConverter::toCsvLine(const Shikigami& shikigami)
{
	std::vector<std::string> csvLineColumns = toCsvLineColumns(shikigami);

	return joinCsvLineColumns(csvLineColumns);
}

std::vector<std::string> ShikigamiCsvConverter::spilitCsvColumns(const std::string& csvLine)
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
	Shikigami shikigami;

	if (csvLineColumns.size() < SHIKIGAMI_CSV_COLUMNCOUNT) {
		return shikigami;
	}

	shikigami.Rarity = csvLineColumns[RARITY_INDEX];
	shikigami.Name = csvLineColumns[NAME_INDEX];
	shikigami.Attack = std::stod(csvLineColumns[ATTACK_INDEX]);
	shikigami.HP = std::stod(csvLineColumns[HP_INDEX]);
	shikigami.Defense = std::stod(csvLineColumns[DEFFENSE_INDEX]);
	shikigami.Speed = std::stod(csvLineColumns[SPEED_INDEX]);
	shikigami.CritRate = std::stod(csvLineColumns[CRIT_RATE_INDEX]);
	shikigami.CritDamage = std::stod(csvLineColumns[CRIT_DAMAGE_INDEX]);
	shikigami.EffectHit = std::stod(csvLineColumns[EFFECT_HIT_INDEX]);
	shikigami.EffectResist = std::stod(csvLineColumns[EFFECT_RESITST_INDEX]);

	return shikigami;
}

std::vector<std::string> ShikigamiCsvConverter::toCsvLineColumns(const Shikigami& shikigami)
{
	std::vector<std::string> csvLineColumns;

	csvLineColumns.push_back(shikigami.Rarity);
	csvLineColumns.push_back(shikigami.Name);
	csvLineColumns.push_back(std::to_string(shikigami.Attack));
	csvLineColumns.push_back(std::to_string(shikigami.HP));
	csvLineColumns.push_back(std::to_string(shikigami.Defense));
	csvLineColumns.push_back(std::to_string(shikigami.Speed));
	csvLineColumns.push_back(std::to_string(shikigami.CritRate));
	csvLineColumns.push_back(std::to_string(shikigami.CritDamage));
	csvLineColumns.push_back(std::to_string(shikigami.EffectHit));
	csvLineColumns.push_back(std::to_string(shikigami.EffectResist));

	return csvLineColumns;
}