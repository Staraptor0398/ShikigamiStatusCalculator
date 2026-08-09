#include "pch.h"
#include "../../Core/Service/ShikigamiService.h"
#include "../Converter/Model/ShikigamiConverter.h"
#include "../Converter/Model/ShikigamiListConverter.h"
#include "../Converter/Model/StringConverter.h"
#include "../Mapper/Outcome/ShikigamiDataOutcomeMapper.h"
#include "ShikigamiGateway.h"

ShikigamiDataOutcomeDto ShikigamiGateway::GetShikigamiList(String^ filePath, List<ShikigamiDto^>^% outShikigamiList)
{
	outShikigamiList = gcnew List<ShikigamiDto^>();

	std::string nativePath = StringConverter::toStdString(filePath);

	std::vector<Shikigami> nativeList;

	ShikigamiDataOutcome outcome = ShikigamiService::getShikigamiList(nativePath, nativeList);

	if (outcome != ShikigamiDataOutcome::SUCCESS) {
		return ShikigamiDataOutcomeMapper::toDto(outcome);
	}

	outShikigamiList = ShikigamiListConverter::toDto(nativeList);

	return ShikigamiDataOutcomeDto::SUCCESS;
}

ShikigamiDataOutcomeDto ShikigamiGateway::AddShikigami(String^ filePath, ShikigamiDto^ dto)
{
	std::string nativePath = StringConverter::toStdString(filePath);

	Shikigami native;

	try {
		native = ShikigamiConverter::toNative(dto);
	}
	catch (System::ArgumentNullException^) {
		return ShikigamiDataOutcomeDto::INVALID_ARGUMENT;
	}
	catch (...) {
		return ShikigamiDataOutcomeDto::UNKNOWN_ERROR;
	}

	ShikigamiDataOutcome outcome = ShikigamiService::addShikigami(nativePath, native);

	return ShikigamiDataOutcomeMapper::toDto(outcome);
}

ShikigamiDataOutcomeDto ShikigamiGateway::UpdateShikigami(String^ filePath, ShikigamiDto^ oldDto, ShikigamiDto^ newDto)
{
	std::string nativePath = StringConverter::toStdString(filePath);

	Shikigami oldData;
	Shikigami newData;

	try {
		oldData = ShikigamiConverter::toNative(oldDto);
		newData = ShikigamiConverter::toNative(newDto);
	}
	catch (System::ArgumentNullException^) {
		return ShikigamiDataOutcomeDto::INVALID_ARGUMENT;
	}
	catch (...) {
		return ShikigamiDataOutcomeDto::UNKNOWN_ERROR;
	}

	ShikigamiDataOutcome outcome = ShikigamiService::updateShikigami(nativePath, oldData, newData);

	return ShikigamiDataOutcomeMapper::toDto(outcome);
}

ShikigamiDataOutcomeDto ShikigamiGateway::GetRecoveryCandinateShikigamiList(String^ currentFilePath, String^ sourceFilePath, List<ShikigamiDto^>^% outRecoveryCandinateShikigamiList)
{

	outRecoveryCandinateShikigamiList = gcnew List<ShikigamiDto^>();

	std::string nativeCurrentPath = StringConverter::toStdString(currentFilePath);

	std::string nativeSourcePath = StringConverter::toStdString(sourceFilePath);

	std::vector<Shikigami> nativeRecoveryCandinateList;

	ShikigamiDataOutcome outcome = ShikigamiService::getRecoveryCandinateShikigamiList(nativeCurrentPath, nativeSourcePath, nativeRecoveryCandinateList);

	if (outcome != ShikigamiDataOutcome::SUCCESS) {
		return ShikigamiDataOutcomeMapper::toDto(outcome);
	}

	outRecoveryCandinateShikigamiList = ShikigamiListConverter::toDto(nativeRecoveryCandinateList);

	return ShikigamiDataOutcomeDto::SUCCESS;
}

ShikigamiDataOutcomeDto ShikigamiGateway::RecoveryShikigami(String^ currentFilePath, List<ShikigamiDto^>^ selectedShikigamiList)
{
	std::string nativeCurrentPath = StringConverter::toStdString(currentFilePath);

	std::vector<Shikigami> nativeList;

	try {
		nativeList = ShikigamiListConverter::toNative(selectedShikigamiList);
	}
	catch (System::ArgumentNullException^) {
		return ShikigamiDataOutcomeDto::INVALID_ARGUMENT;
	}
	catch (...) {
		return ShikigamiDataOutcomeDto::UNKNOWN_ERROR;
	}

	ShikigamiDataOutcome outcome = ShikigamiService::recoverShikigamiList(nativeCurrentPath, nativeList);

	return ShikigamiDataOutcomeMapper::toDto(outcome);
}
