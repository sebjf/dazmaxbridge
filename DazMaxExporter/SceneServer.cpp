#include "SceneServer.h"

TCHAR sharedMemoryName[] = TEXT("Local\\DazMaxBridgeSharedMemory");

void MySceneServer::createSharedMemory(DWORD BUF_SIZE)
{
	   hMapFile = CreateFileMapping(
					 INVALID_HANDLE_VALUE,    // use paging file
					 NULL,                    // default security
					 PAGE_READWRITE,          // read/write access
					 0,                       // maximum object size (high-order DWORD)
					 BUF_SIZE,                // maximum object size (low-order DWORD)
					 sharedMemoryName);       // name of mapping object

	   if (hMapFile == NULL)
	   {
		  DWORD error =  GetLastError();
		  dzApp->log(QString("Could not created shared memory with name: %i (Error: %2)\n").arg(sharedMemoryName,QString::number(error)));
		  return;
	   }
	   pBuf = MapViewOfFile(hMapFile,   // handle to map object
					FILE_MAP_ALL_ACCESS, // read/write permission
					0,
					0,
					BUF_SIZE);

	   if (pBuf == NULL)
	   {
		   DWORD error =  GetLastError();
		   dzApp->log(QString("Could not create view of shared memory %i (Error: %2)\n").arg(sharedMemoryName,QString::number(error)));
		   CloseHandle(hMapFile);
		   return;
	   }
}

MySceneServer::~MySceneServer()
{
	UnmapViewOfFile(pBuf);
	CloseHandle(hMapFile);
}

void 	MySceneServer::executeAction()
{
	MyDazExporter exporter;
	msgpack::sbuffer sbuf;
	exporter.write(sbuf, 0);
	
	memcpy(pBuf,sbuf.data(),sbuf.size());

	QMessageBox myMessageBox;
	myMessageBox.setText("Complete!");
	myMessageBox.exec();	
}

void 	MySceneServer::toggleAction(bool onOff)
{

}