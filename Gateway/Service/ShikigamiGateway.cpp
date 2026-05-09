#include "pch.h"
#include "ShikigamiGateway.h"
#include "../Converter/Model/StringConverter.h"
#include "../Converter/Model/ShikigamiConverter.h"
#include "../Mapper/Outcome/ShikigamiDataOutcomeMapper.h"
#include "../../Core/Shikigami/ShikigamiRepository.h"


ShikigamiDataOutcomeDto ShikigamiGateway::GetShikigamiList(String^ filePath, List<ShikigamiDto^>^% outShikigamiList)
{
	outShikigamiList = gcnew List<ShikigamiDto^>();

	std::string nativePath = StringConverter::toStdString(filePath);

	std::vector<Shikigami> nativeList;

	ShikigamiDataOutcome outcome = ShikigamiRepository::get_ShikigamiList(nativePath, nativeList);

	if (outcome != ShikigamiDataOutcome::SUCCESS) {
		return ShikigamiDataOutcomeMapper::toDto(outcome);
	}

	for (auto& shikigami : nativeList) {
		outShikigamiList->Add(ShikigamiConverter::toDto(shikigami));
	}

	return ShikigamiDataOutcomeDto::SUCCESS;
}

ShikigamiDataOutcomeDto ShikigamiGateway::AddShikigami(String^ filePath, ShikigamiDto^ dto)
{
	std::string nativePath = StringConverter::toStdString(filePath);

	Shikigami native = ShikigamiConverter::toNative(dto);

	ShikigamiDataOutcome outcome = ShikigamiRepository::add_Shikigami(nativePath, native);

	return ShikigamiDataOutcomeMapper::toDto(outcome);
}

ShikigamiDataOutcomeDto ShikigamiGateway::UpdateShikigami(String^ filePath, ShikigamiDto^ oldDto, ShikigamiDto^ newDto)
{
	std::string nativePath = StringConverter::toStdString(filePath);

	Shikigami oldData = ShikigamiConverter::toNative(oldDto);

	Shikigami newData = ShikigamiConverter::toNative(newDto);

	ShikigamiDataOutcome outcome = ShikigamiRepository::update_Shikigami(nativePath, oldData, newData);

	return ShikigamiDataOutcomeMapper::toDto(outcome);
}