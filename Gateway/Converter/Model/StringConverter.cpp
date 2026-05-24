#include "pch.h"
#include "StringConverter.h"

#include <msclr/marshal_cppstd.h>

using namespace System::Text;

String^ StringConverter::toManagedString(const std::string& text)
{
	array<Byte>^ bytes = gcnew array<Byte>((int)text.size());

	for (int i = 0; i < (int)text.size(); i++) {
		bytes[i] = (Byte)text[i];
	}

	return Encoding::UTF8->GetString(bytes);
}

std::string StringConverter::toStdString(String^ text)
{
	if (text == nullptr) {
		return "";
	}

	return msclr::interop::marshal_as<std::string>(text);
}

std::string StringConverter::toUtf8String(String^ text)
{
	if (text == nullptr) {
		return "";
	}

	array<Byte>^ bytes = Encoding::UTF8->GetBytes(text);

	std::string result;
	result.reserve(bytes->Length);

	for (int i = 0; i < bytes->Length; i++) {
		result.push_back(bytes[i]);
	}

	return result;
}
