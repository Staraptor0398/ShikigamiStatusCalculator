#pragma once

#include <filesystem>
#include <fstream>
#include <nlohmann/json.hpp>
#include <stdexcept>
#include <string>

class JsonDataAccess
{
public:
	template<typename T, typename TConverter>
	static T load(const std::filesystem::path& filePath) {
		std::ifstream file(filePath);

		if (!file.is_open()) {
			throw std::runtime_error("Failed to open JSON file: " + filePath.string());
		}

		nlohmann::json json;
		file >> json;

		return TConverter::toTestData(json);
	}
};
