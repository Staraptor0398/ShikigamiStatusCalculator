#pragma once
#include<string>

class ShikigamiRarity
{
public:
	explicit ShikigamiRarity();
	explicit ShikigamiRarity(const std::string& value);

	const std::string toString()const;

	ShikigamiRarity& operator=(const std::string& value);
	bool operator==(const ShikigamiRarity& other) const;
	bool operator<(const ShikigamiRarity& other) const;

private:
	void setValue(const std::string& value);

private:
	enum class Type
	{
		Other,
		SR,
		SSR,
		SP,
		UR
	};

	std::string mValue;
	Type mType;

};
