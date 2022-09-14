#pragma once

#ifdef NEEVIEWINTEROP_EXPORTS
#define  NEEVIEWINTEROP_API __declspec(dllexport)
#else
#define  NEEVIEWINTEROP_API __declspec(dllimport)
#endif

extern "C" NEEVIEWINTEROP_API bool __stdcall NVGetImageCodecInfo(unsigned int index, wchar_t* friendlyName, wchar_t* fileExtensions);
extern "C" NEEVIEWINTEROP_API void __stdcall NVCloseImageCodecInfo();
extern "C" NEEVIEWINTEROP_API void __stdcall NVFpReset();
extern "C" NEEVIEWINTEROP_API bool __stdcall NVGetFullPathFromShortcut(const wchar_t* shortcut, wchar_t* fullPath);
