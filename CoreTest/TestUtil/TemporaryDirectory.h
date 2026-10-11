#pragma once

#include <filesystem>

class TemporaryDirectory
{
public:
	TemporaryDirectory();
	~TemporaryDirectory();

	TemporaryDirectory(const TemporaryDirectory&) = delete;
	TemporaryDirectory& operator=(const TemporaryDirectory&) = delete;

	const std::filesystem::path& getPath() const;

private:
	std::filesystem::path mPath;
};
