#include "pch.h"
#include "ShikigamiDataOutcomeMapper.h"

ShikigamiDataOutcomeDto ShikigamiDataOutcomeMapper::toDto(ShikigamiDataOutcome outcome)
{
	switch (outcome) {
		case ShikigamiDataOutcome::SUCCESS:					return ShikigamiDataOutcomeDto::SUCCESS;
		case ShikigamiDataOutcome::FILE_OPEN_FAILED:		return ShikigamiDataOutcomeDto::FILE_OPEN_FAILED;
		case ShikigamiDataOutcome::FILE_READ_FAILED:		return ShikigamiDataOutcomeDto::FILE_READ_FAILED;
		case ShikigamiDataOutcome::FILE_WRITE_FAILED:		return ShikigamiDataOutcomeDto::FILE_WRITE_FAILED;
		case ShikigamiDataOutcome::INVALID_FORMAT:			return ShikigamiDataOutcomeDto::INVALID_FORMAT;
		case ShikigamiDataOutcome::NOT_FOUND:				return ShikigamiDataOutcomeDto::NOT_FOUND;
		case ShikigamiDataOutcome::DUPLICATE:				return ShikigamiDataOutcomeDto::DUPLICATE;
		case ShikigamiDataOutcome::UNKNOWN_ERROR:
		default:											return ShikigamiDataOutcomeDto::UNKNOWN_ERROR;
	}
}
