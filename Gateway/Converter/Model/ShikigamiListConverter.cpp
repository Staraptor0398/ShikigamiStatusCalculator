#include "pch.h"
#include "ShikigamiListConverter.h"

#include "ShikigamiConverter.h"

std::vector<Shikigami> ShikigamiListConverter::toNative(List<ShikigamiDto^>^ dto)
{
	if (dto == nullptr) {
		throw gcnew System::ArgumentNullException("dto", "ShikigamiDto list must not be null.");
	}

	std::vector<Shikigami> native;

	for each (ShikigamiDto ^ shikigami in dto)
	{
		native.push_back(ShikigamiConverter::toNative(shikigami));
	}

	return native;
}

List<ShikigamiDto^>^ ShikigamiListConverter::toDto(std::vector<Shikigami> native)
{
	List<ShikigamiDto^>^ dto = gcnew List<ShikigamiDto^>();
	for (const auto& shikigami : native) {
		dto->Add(ShikigamiConverter::toDto(shikigami));
	}

	return dto;
}
