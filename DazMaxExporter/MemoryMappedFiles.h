#ifndef MEMORYMAPPEDFILES_H_
#define MEMORYMAPPEDFILES_H_

#include "Types.h"

#include <windows.h>
#include <stdio.h>

class MemoryMappedFile
{
public:
	MemoryMappedFile();
	~MemoryMappedFile();

	QString name;
	long size;

	/*Force the user to go through this to get the pointer and theres no chance of them using an undersized file...*/

	void* open(int size);

private:
	void* ptr;
	HANDLE handle;

};

#pragma warning(push)
#pragma warning( disable : 4267)

namespace msgpack {

/*
typedef struct msgpack_sbuffer {
	size_t size;
	char* data;
	size_t alloc;
} msgpack_sbuffer;
*/

class sharedmembuffer : public msgpack_sbuffer {

private:
	typedef msgpack_sbuffer base;

public:
	MemoryMappedFile sharedMemory;

public:
	sharedmembuffer(size_t initsz = MSGPACK_SBUFFER_INIT_SIZE)
	{
		base::data = (char*)sharedMemory.open(initsz);
		if(!base::data) {
			throw std::bad_alloc();
		}

		base::size = 0;
		base::alloc = initsz;
	}

	~sharedmembuffer()
	{

	}

public:
	void write(const char* buf, unsigned int len)
	{
		if(base::alloc - base::size < len) {
			expand_buffer(len);
		}
		memcpy(base::data + base::size, buf, len);
		base::size += len;
	}

	char* data()
	{
		return base::data;
	}

	const char* data() const
	{
		return base::data;
	}

	size_t size() const
	{
		return base::size;
	}

	char* release()
	{
		return msgpack_sbuffer_release(this);
	}

	void clear()
	{
		msgpack_sbuffer_clear(this);
	}

private:
	void expand_buffer(size_t len)
	{
		size_t nsize = (base::alloc) ?
				base::alloc * 2 : MSGPACK_SBUFFER_INIT_SIZE;
	
		while(nsize < base::size + len) { nsize *= 2; }
	


		void* tmp = sharedMemory.open(nsize);
		if(!tmp) {
			throw std::bad_alloc();
		}

		memcpy(tmp,base::data,base::size);
	
		base::data = (char*)tmp;
		base::alloc = nsize;
	}

};

#pragma warning(pop)


}  // namespace msgpack

#endif 

