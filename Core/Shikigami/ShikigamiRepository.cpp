#include "pch.h"
#include "ShikigamiRepository.h"

#include "../IO/FileAccess.h"
#include "../Mapper/Outcome/FileAccessOutcomeMapper.h"

std::string ShikigamiRepository::mRecoverySourceFilePath;

ShikigamiDataOutcome ShikigamiRepository::getShikigamiList(const std::string& filePath, std::vector<Shikigami>& outShikigamis)
{
	FileAccessOutcome outcome = FileAccess::loadShikigami(filePath, outShikigamis);

	return FileAccessOutcomeMapper::toShikigamiDataOutcome(outcome);
}

ShikigamiDataOutcome ShikigamiRepository::addShikigami(const std::string& filePath, const Shikigami& newData)
{
	std::vector<Shikigami> shikigamis;

	FileAccessOutcome loadOutcome = FileAccess::loadShikigami(filePath, shikigamis);

	if (loadOutcome != FileAccessOutcome::SUCCESS) {
		return FileAccessOutcomeMapper::toShikigamiDataOutcome(loadOutcome);
	}

	if (existsSameShikigami(shikigamis, newData)) {
		return ShikigamiDataOutcome::DUPLICATE;
	}

	int insertIndex = findInsertIndex(shikigamis, newData);

	shikigamis.insert(shikigamis.begin() + insertIndex, newData);

	FileAccessOutcome saveOutcome = FileAccess::saveShikigami(filePath, shikigamis);

	return FileAccessOutcomeMapper::toShikigamiDataOutcome(saveOutcome);
}

ShikigamiDataOutcome ShikigamiRepository::updateShikigami(const std::string& filePath, const Shikigami& oldData, const Shikigami& newData)
{
	std::vector<Shikigami> shikigamis;

	FileAccessOutcome loadOutcome = FileAccess::loadShikigami(filePath, shikigamis);

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

	FileAccessOutcome saveOutcome = FileAccess::saveShikigami(filePath, shikigamis);

	return FileAccessOutcomeMapper::toShikigamiDataOutcome(saveOutcome);
}

ShikigamiDataOutcome ShikigamiRepository::getRecoveryCandinateShikigamiList(const std::string& currentFilePath, const std::string& sourceFilePath, std::vector<Shikigami>& outRecoveryCandinateShikigamis)
{
	std::vector<Shikigami> currentShikigamis;

	FileAccessOutcome loadOutcome = FileAccess::loadShikigami(currentFilePath, currentShikigamis);

	if (loadOutcome != FileAccessOutcome::SUCCESS) {
		return FileAccessOutcomeMapper::toShikigamiDataOutcome(loadOutcome);
	}

	std::vector<Shikigami> sourceShikigamis;

	loadOutcome = FileAccess::loadValidShikigami(sourceFilePath, sourceShikigamis);

	if (loadOutcome != FileAccessOutcome::SUCCESS) {
		return FileAccessOutcomeMapper::toShikigamiDataOutcome(loadOutcome);
	}

	mRecoverySourceFilePath = sourceFilePath;

	outRecoveryCandinateShikigamis = extractUnknownShikigamiList(currentShikigamis, sourceShikigamis);

	return ShikigamiDataOutcome::SUCCESS;
}

ShikigamiDataOutcome ShikigamiRepository::recoverShikigamiList(const std::string& currentFilePath, const std::vector<Shikigami>& selectedShikigamis)
{
	std::vector<Shikigami> currentShikigamis;

	FileAccessOutcome loadOutcome = FileAccess::loadShikigami(currentFilePath, currentShikigamis);

	if (loadOutcome != FileAccessOutcome::SUCCESS) {
		return FileAccessOutcomeMapper::toShikigamiDataOutcome(loadOutcome);
	}

	std::vector<Shikigami> sourceShikigamis;

	loadOutcome = FileAccess::loadValidShikigami(mRecoverySourceFilePath, sourceShikigamis);

	if (loadOutcome != FileAccessOutcome::SUCCESS) {
		return FileAccessOutcomeMapper::toShikigamiDataOutcome(loadOutcome);
	}

	std::vector<Shikigami> recoveryTargetShikigamis = extractUnknownShikigamiList(currentShikigamis, selectedShikigamis);

	if (recoveryTargetShikigamis.empty()) {
		return ShikigamiDataOutcome::SUCCESS;
	}

	std::vector<Shikigami> mergedShikigamis = mergeShikigamiList(currentShikigamis, sourceShikigamis, recoveryTargetShikigamis);

	FileAccessOutcome saveOutcome = FileAccess::saveShikigami(currentFilePath, mergedShikigamis);

	if (saveOutcome == FileAccessOutcome::SUCCESS) {
		mRecoverySourceFilePath.clear();
	}

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

		if (isSameShikigami(shikigami, newData)) {
			return true;
		}
	}

	return false;
}

int ShikigamiRepository::findInsertIndex(const std::vector<Shikigami>& shikigamis, const Shikigami& newData)
{
	for (int i = 0;i < static_cast<int>(shikigamis.size());i++) {
		if (shikigamis[i].Rarity < newData.Rarity) {
			return i;
		}
	}

	return static_cast<int>(shikigamis.size());
}

int ShikigamiRepository::findMergeInsertIndex(const std::vector<Shikigami>& mergedShikigamis, const std::vector<Shikigami>& sourceShikigamis, const Shikigami& target) {
	int sourceIndex = -1;

	for (int i = 0;i < static_cast<int>(sourceShikigamis.size());i++) {
		if (isSameShikigami(sourceShikigamis[i], target)) {
			sourceIndex = i;
			break;
		}
	}

	if (sourceIndex < 0) {
		return findInsertIndex(mergedShikigamis, target);
	}

	// 復旧元上で target より前にある、
	// merged 側にも存在する最寄りの式神を探す
	for (int i = sourceIndex - 1;i >= 0;i--) {
		for (int j = 0;j < static_cast<int>(mergedShikigamis.size());j++) {
			if (isSameShikigami(sourceShikigamis[i], mergedShikigamis[j])) {
				return j + 1;
			}
		}
	}

	// 前方にアンカーがなければ、
	// target より後ろにある最寄りの式神を探す
	for (int i = sourceIndex + 1;i < static_cast<int>(sourceShikigamis.size());i++) {
		for (int j = 0;j < static_cast<int>(mergedShikigamis.size());j++) {
			if (isSameShikigami(sourceShikigamis[i], mergedShikigamis[j])) {
				return j;
			}
		}
	}

	// 共通する式神が一件もなければ既存ルールへフォールバック
	return findInsertIndex(mergedShikigamis, target);
}

std::vector<Shikigami> ShikigamiRepository::extractUnknownShikigamiList(const std::vector<Shikigami>& currentShikigamis, const std::vector<Shikigami>& sourceShikigamis)
{
	std::vector<Shikigami> extractedShikigamis;

	for (const auto& sourceShikigami : sourceShikigamis) {
		if (!existsSameShikigami(currentShikigamis, sourceShikigami)) {
			extractedShikigamis.push_back(sourceShikigami);
		}
	}

	return extractedShikigamis;
}

std::vector<Shikigami> ShikigamiRepository::mergeShikigamiList(const std::vector<Shikigami>& currentShikigamis, const std::vector<Shikigami>& sourceShikigamis, const std::vector<Shikigami>& recoveryTargetShikigamis)
{
	std::vector<Shikigami> mergedShikigamis;

	mergedShikigamis = currentShikigamis;

	for (const auto& sourceShikigami : sourceShikigamis) {
		if (!existsSameShikigami(recoveryTargetShikigamis, sourceShikigami)) {
			continue;
		}

		int insertIndex = findMergeInsertIndex(mergedShikigamis, sourceShikigamis, sourceShikigami);

		mergedShikigamis.insert(mergedShikigamis.begin() + insertIndex, sourceShikigami);
	}

	return mergedShikigamis;
}
