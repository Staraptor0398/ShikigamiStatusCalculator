#include "pch.h"
#include "ShikigamiRarity.h"

ShikigamiRarity::ShikigamiRarity()
{
	setValue("");
}

ShikigamiRarity::ShikigamiRarity(const std::string& value)
{
	setValue(value);
}

const std::string ShikigamiRarity::toString() const
{
	return mValue;
}

ShikigamiRarity& ShikigamiRarity::operator=(const std::string& value)
{
	setValue(value);
	return *this;
}

bool ShikigamiRarity::operator==(const ShikigamiRarity& other) const
{
	return mValue == other.mValue;
}

bool ShikigamiRarity::operator<(const ShikigamiRarity& other) const
{
	if (mType == Type::Other && other.mType == Type::Other) {
		return false;
	}

	return mType < other.mType;
}

void ShikigamiRarity::setValue(const std::string& value)
{
	mValue = value;

	if (value == "UR") {
		mType = Type::UR;
	}
	else if (value == "SP") {
		mType = Type::SP;
	}
	else if (value == "SSR") {
		mType = Type::SSR;
	}
	else if (value == "SR") {
		mType = Type::SR;
	}
	else {
		mType = Type::Other;
	}
}

