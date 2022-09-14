// from Stack Overflow 
// URL: https://ja.stackoverflow.com/questions/44449/bitmapdecoder-%e3%81%8c%e3%82%b5%e3%83%9d%e3%83%bc%e3%83%88%e3%81%97%e3%81%a6%e3%81%84%e3%82%8b%e7%94%bb%e5%83%8f%e3%83%95%e3%82%a1%e3%82%a4%e3%83%ab%e3%81%ae%e7%a8%ae%e9%a1%9e%e6%8b%a1%e5%bc%b5%e5%ad%90%e3%82%92%e5%85%a8%e3%81%a6%e5%8f%96%e5%be%97%e3%81%97%e3%81%9f%e3%81%84-heic%e3%81%a8%e3%81%8b/44459#44459
// License: CC BY-SA 3.0

#include "stdafx.h"
#include "ImageCodecQuery.h"

#include <wrl/client.h>
#include <wincodec.h>
#include <iostream>
#include <iterator>


template<class T> using ComPtr = Microsoft::WRL::ComPtr<T>;
void check(HRESULT hr) { if (FAILED(hr)) throw hr; }


ImageCodecQuery::ImageCodecQuery()
{
}

ImageCodecQuery::~ImageCodecQuery()
{
}

void ImageCodecQuery::Add(CodecInfo item)
{
	items.push_back(item);
}

CodecInfo* ImageCodecQuery::Get(unsigned int index)
{
	if (index < 0 || index >= items.size()) return nullptr;

	return &items.at(index);
}

void ImageCodecQuery::Dump()
{
	for (auto itr = items.begin(), end_ = items.end(); itr != end_; itr++) {
		std::wcout << itr->friendlyName << L": " << itr->fileExtensions << std::endl;
	}
}

void ImageCodecQuery::Initialize()
{
	ComPtr<IWICImagingFactory> imageingFactory;
	check(CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_ALL, IID_PPV_ARGS(&imageingFactory)));

	ComPtr<IEnumUnknown> enumUnknown;
	check(imageingFactory->CreateComponentEnumerator(WICDecoder, WICComponentEnumerateDefault, &enumUnknown));

	for (ComPtr<IUnknown> unknown; enumUnknown->Next(1, &unknown, nullptr) == S_OK;)
	{
		ComPtr<IWICBitmapCodecInfo> codecInfo;
		check(unknown.As(&codecInfo));
		CodecInfo item;
		UINT actual;

		codecInfo->GetFriendlyName(0, NULL, &actual);
		if (actual >= CodecInfo::MaxLength - 1) throw E_OUTOFMEMORY;
		check(codecInfo->GetFriendlyName(static_cast<UINT>(std::size(item.friendlyName)), item.friendlyName, &actual));

		codecInfo->GetFileExtensions(0, NULL, &actual);
		if (actual >= CodecInfo::MaxLength - 1) throw E_OUTOFMEMORY;
		check(codecInfo->GetFileExtensions(static_cast<UINT>(std::size(item.fileExtensions)), item.fileExtensions, &actual));

		// アンインストールしてもコーデック情報が残っていることがあるのでデコーダーの生成までチェックしてみる
		ComPtr<IWICBitmapDecoderInfo> decoderInfo;
		check(unknown.As(&decoderInfo));
		IWICBitmapDecoder *decoder = NULL;
		HRESULT hr = decoderInfo->CreateInstance(&decoder);
		if (hr == S_OK)
		{
			decoder->Release();
			Add(item);
		}
	}
}

