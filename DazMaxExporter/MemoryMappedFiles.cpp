#include "MemoryMappedFiles.h"

TCHAR sharedMemoryName[] = TEXT("Local\\DazMaxBridgeSharedMemory");

MemoryMappedFile::MemoryMappedFile()
{
	this->ptr = 0;
	this->size = 0;
	this->handle = NULL;
}

void* MemoryMappedFile::open(int size)
{
	if(this->size < size)
	{
		if(handle != NULL)
		{
			CloseHandle(handle);
			ptr = 0;
			handle = NULL;
		}
	}

	if(ptr != 0)
	{
		return ptr;
	}

	name = QString(sharedMemoryName) + "_" + QUuid::createUuid().toString();
	LPCWSTR fname = (LPCWSTR)malloc(512);
	
	name.toWCharArray((wchar_t*)fname);
	handle = CreateFileMappingW(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, size, name.toStdWString().c_str() );
	ptr = MapViewOfFile(handle, FILE_MAP_READ|FILE_MAP_WRITE, 0, 0, size);

	if(ptr > 0)
	{
		this->size = size;
	}

	return ptr;
}
