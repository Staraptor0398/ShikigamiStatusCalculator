#include "pch.h"
#include "TemporaryDirectory.h"

#include <Windows.h>
#include <stdexcept>
#include <system_error>

TemporaryDirectory::TemporaryDirectory()
{
	wchar_t temporaryDirectoryPath[MAX_PATH];

	DWORD length = GetTempPathW(MAX_PATH, temporaryDirectoryPath);

	if (length == 0 || length >= MAX_PATH) {
		throw std::runtime_error("Failed to get temporary directory path.");
	}

	wchar_t temporaryFilePath[MAX_PATH];

	if (GetTempFileNameW(temporaryDirectoryPath, L"CTT", 0, temporaryFilePath) == 0) {
		throw std::runtime_error("Failed to create temporary directory path.");
	}

	mPath = temporaryFilePath;

	std::error_code error;

	if (!std::filesystem::remove(mPath, error) || error) {
		throw std::runtime_error("Failed to remove temporary placeholder file.");
	}

	if (!std::filesystem::create_directory(mPath, error) || error) {
		throw std::runtime_error("Failed to create temporary directory.");
	}
}

TemporaryDirectory::~TemporaryDirectory()
{
	std::error_code error;
	std::filesystem::remove_all(mPath, error);
}

const std::filesystem::path& TemporaryDirectory::getPath() const
{
	return mPath;
}
