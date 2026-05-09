#pragma once

public enum class ShikigamiDataOutcomeDto
{
	SUCCESS,
	FILE_OPEN_FAILED,
	FILE_READ_FAILED,
	FILE_WRITE_FAILED,
	INVALID_FORMAT,
	NOT_FOUND,
	DUPLICATE,
	UNKNOWN_ERROR
};