#pragma once

#include <vector>

struct CodecInfo
{
	const static int MaxLength = 1024;

	wchar_t friendlyName[MaxLength];
	wchar_t fileExtensions[MaxLength];
};

class ImageCodecQuery
{
private:
		std::vector<CodecInfo> items;

public:
	ImageCodecQuery();
	~ImageCodecQuery();

	void Add(CodecInfo item);
	CodecInfo* Get(unsigned int index);
	void Dump();
	void Initialize();
};

