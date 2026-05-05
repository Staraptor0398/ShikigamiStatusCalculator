#include "pch.h"
#include "ShikigamiGateway.h"
#include "../Converter/StringConverter.h"
#include "../Converter/ShikigamiConverter.h"
#include "../../Core/Shikigami/ShikigamiRepository.h"


List<ShikigamiDto^>^ ShikigamiGateway::GetShikigamiList(System::String^ filePath)
{
	std::string nativePath = StringConverter::toStdString(filePath);
	auto nativaList = ShikigamiRepository::get_ShikigamiList(nativePath);

	List<ShikigamiDto^>^ result = gcnew List<ShikigamiDto^>();

	for (auto& s : nativaList) {
		result->Add(ShikigamiConverter::toDto(s));
	}

	return result;
}

void ShikigamiGateway::AddShikigami(String^ filePath, ShikigamiDto^ dto)
{
	std::string nativePath = StringConverter::toStdString(filePath);

	Shikigami native = ShikigamiConverter::toNative(dto);

	ShikigamiRepository::add_Shikigami(nativePath, native);
}