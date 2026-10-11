#include "pch.h"
#include "FileAccess.h"

#include "../Converter/Format/ShikigamiCsvConverter.h"
#include <Windows.h>
#include <filesystem>
#include <fstream>

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

		try {
			outShikigamis.push_back(ShikigamiCsvConverter::toShikigami(line));
		}
		catch (const std::invalid_argument&) {
			outShikigamis.clear();
			return FileAccessOutcome::INVALID_FORMAT;
		}
		catch (const std::out_of_range&) {
			outShikigamis.clear();
			return FileAccessOutcome::INVALID_FORMAT;
		}
		catch (...) {
			outShikigamis.clear();
			return FileAccessOutcome::UNKNOWN_ERROR;
		}
	}

	if (file.bad()) {
		outShikigamis.clear();
		return FileAccessOutcome::FILE_READ_FAILED;
	}

	return FileAccessOutcome::SUCCESS;
}

FileAccessOutcome FileAccess::loadValidShikigami(const std::string& filePath, std::vector<Shikigami>& outShikigamis)
{
	outShikigamis.clear();

	std::ifstream file(filePath);

	if (!file.is_open()) {
		return FileAccessOutcome::FILE_OPEN_FAILED;
	}

	std::string line;

	while (std::getline(file, line)) {
		if (line.empty()) {
			continue;
		}

		try {
			outShikigamis.push_back(ShikigamiCsvConverter::toShikigami(line));
		}
		catch (const std::invalid_argument&) {
			continue;
		}
		catch (const std::out_of_range&) {
			continue;
		}
		catch (...) {
			outShikigamis.clear();
			return FileAccessOutcome::UNKNOWN_ERROR;
		}
	}

	if (file.bad()) {
		outShikigamis.clear();
		return FileAccessOutcome::FILE_READ_FAILED;
	}

	return FileAccessOutcome::SUCCESS;
}

FileAccessOutcome FileAccess::saveShikigami(const std::string& filePath, const std::vector<Shikigami>& shikigamis)
{
	std::filesystem::path destinationPath(filePath);
	std::filesystem::path directoryPath = destinationPath.parent_path();

	if (directoryPath.empty()) {
		directoryPath = L".";
	}

	wchar_t temporaryFileName[MAX_PATH];

	if (GetTempFileNameW(directoryPath.c_str(), L"SHK", 0, temporaryFileName) == 0) {
		return FileAccessOutcome::FILE_OPEN_FAILED;
	}

	std::filesystem::path temporaryPath(temporaryFileName);

	try {
		std::ofstream file(temporaryPath, std::ios::trunc);

		if (!file.is_open()) {
			DeleteFileW(temporaryPath.c_str());
			return FileAccessOutcome::FILE_OPEN_FAILED;
		}

		// 既存CSVは変更せず、一時ファイルへ全データを書き込む。
		file << SHIKIGAMI_CSV_HEADER << std::endl;

		for (const auto& shikigami : shikigamis) {
			file << ShikigamiCsvConverter::toCsvLine(shikigami) << std::endl;
		}

		file.flush();

		if (!file) {
			file.close();
			DeleteFileW(temporaryPath.c_str());
			return FileAccessOutcome::FILE_WRITE_FAILED;
		}

		file.close();

		if (!file) {
			DeleteFileW(temporaryPath.c_str());
			return FileAccessOutcome::FILE_WRITE_FAILED;
		}
	}
	catch (...) {
		DeleteFileW(temporaryPath.c_str());
		return FileAccessOutcome::UNKNOWN_ERROR;
	}

	// 一時ファイルの書き込みが完了してから既存CSVを置換する。
	if (!ReplaceFileW(destinationPath.c_str(), temporaryPath.c_str(), nullptr, 0, nullptr, nullptr)) {
		// 置換失敗時に元CSVが存在しなければ、
		// 復旧に使用できるよう一時ファイルを残す。
		if (GetFileAttributesW(destinationPath.c_str()) != INVALID_FILE_ATTRIBUTES) {
			DeleteFileW(temporaryPath.c_str());
		}

		return FileAccessOutcome::FILE_WRITE_FAILED;
	}

	return FileAccessOutcome::SUCCESS;
}
