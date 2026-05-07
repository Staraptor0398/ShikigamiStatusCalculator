#include "pch.h"
#include "ShikigamiGateway.h"
#include "../Converter/Model/StringConverter.h"
#include "../Converter/Model/ShikigamiConverter.h"
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

void ShikigamiGateway::UpdateShikigami(String^ filePath, ShikigamiDto^ oldDto, ShikigamiDto^ newDto)
{
	std::string nativePath = StringConverter::toStdString(filePath);

	Shikigami oldData = ShikigamiConverter::toNative(oldDto);

	Shikigami newData = ShikigamiConverter::toNative(newDto);

	ShikigamiRepository::update_Shikigami(nativePath, oldData, newData);
}