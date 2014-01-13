#include "DazMaxExporter.h"
#include <windows.h>
#include <stdio.h>

class MemoryMappedFile
{
public:
	MemoryMappedFile();

	QString name;
	long size;

	/*Force the user to go through this to get the pointer and theres no chance of them using an undersized file...*/

	void* open(int size);

private:
	void* ptr;
	HANDLE handle;

};