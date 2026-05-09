#include "pch.h"
#include "ShikigamiRepository.h"

#include "../IO/FileAccess.h"
#include "../Mapper/Outcome/FileAccessOutcomeMapper.h"

ShikigamiDataOutcome ShikigamiRepository::get_ShikigamiList(const std::string& filePath, std::vector<Shikigami>& outShikigamis)
{
	FileAccessOutcome outcome = FileAccess::load_Shikigami(filePath, outShikigamis);

	return FileAccessOutcomeMapper::toShikigamiDataOutcome(outcome);
}

ShikigamiDataOutcome ShikigamiRepository::add_Shikigami(const std::string& filePath, const Shikigami& newData)
{
	std::vector<Shikigami> shikigamis;

	FileAccessOutcome loadOutcome = FileAccess::load_Shikigami(filePath, shikigamis);

	if (loadOutcome != FileAccessOutcome::SUCCESS) {
		return FileAccessOutcomeMapper::toShikigamiDataOutcome(loadOutcome);
	}

	if (existsSameShikigami(shikigamis, newData)) {
		return ShikigamiDataOutcome::DUPLICATE;
	}

	int insertIndex = findInsertIndex(shikigamis, newData);

	shikigamis.insert(shikigamis.begin() + insertIndex, newData);

	FileAccessOutcome saveOutcome = FileAccess::save_Shikigami(filePath, shikigamis);

	return FileAccessOutcomeMapper::toShikigamiDataOutcome(saveOutcome);
}

ShikigamiDataOutcome ShikigamiRepository::update_Shikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData)
{
	std::vector<Shikigami> shikigamis;

	FileAccessOutcome loadOutcome = FileAccess::load_Shikigami(filePath, shikigamis);

	if (loadOutcome != FileAccessOutcome::SUCCESS) {
		return FileAccessOutcomeMapper::toShikigamiDataOutcome(loadOutcome);
	}

	if (!existsSameShikigami(shikigamis, oldData)) {
		return ShikigamiDataOutcome::NOT_FOUND;
	}

	if (existsSameShikigamiExceptSelf(shikigamis, oldData, newData)) {
		return ShikigamiDataOutcome::DUPLICATE;
	}

	for (auto& shikigami : shikigamis) {
		if (isSameShikigami(shikigami, oldData)) {
			shikigami = newData;
			break;
		}
	}

	FileAccessOutcome saveOutcome = FileAccess::save_Shikigami(filePath, shikigamis);

	return FileAccessOutcomeMapper::toShikigamiDataOutcome(saveOutcome);
}

bool ShikigamiRepository::isSameShikigami(const Shikigami& left, const Shikigami& right)
{
	return left.Rarity == right.Rarity && left.Name == right.Name;
}

bool ShikigamiRepository::existsSameShikigami(const std::vector<Shikigami>& shikigamis, const Shikigami& target)
{
	for (const auto& shikigami : shikigamis) {
		if (isSameShikigami(shikigami, target)) {
			return true;
		}
	}

	return false;
}

bool ShikigamiRepository::existsSameShikigamiExceptSelf(const std::vector<Shikigami>& shikigamis, const Shikigami& oldData, const Shikigami& newData)
{
	for (const auto& shikigami : shikigamis) {
		if (isSameShikigami(shikigami, oldData)) {
			continue;
		}

		if (isSameShikigami(shikigami, oldData)) {
			return true;
		}
	}

	return false;
}

int ShikigamiRepository::findInsertIndex(const std::vector<Shikigami>& shikigamis, const Shikigami& newData)
{
	int insertIndex = static_cast<int>(shikigamis.size());

	for (int i = 0;i < static_cast<int>(shikigamis.size());i++) {
		if (shikigamis[i].Rarity == newData.Rarity) {
			insertIndex = i + 1;
		}
	}

	return insertIndex;
}