#include "SceneServer.h"
#include <Winuser.h>
#include <tchar.h>

TCHAR sharedMemoryName[] = TEXT("Local\\DazMaxBridgeSharedMemory");
TCHAR namedPipeName[] = TEXT("DazMaxBridgePipe");

void MySceneServer::startServer()
{
	m_server = new QLocalServer(this);
	if(!m_server->listen(namedPipeName)){
		ShowMessage("Unable to created named pipe.");
	}

	connect(m_server,SIGNAL(newConnection()),this,SLOT(newConnection()));
}

void MySceneServer::newConnection()
{
	m_socket = m_server->nextPendingConnection();
	connect(m_socket,SIGNAL(readyRead()),this,SLOT(messageReceived()));
}

void MySceneServer::messageReceived()
{
	QString message = m_socket->readLine().trimmed();

	if(message == "Hello")
	{
		ShowMessage("Ping from Max");
		return;
	}

	if(message == "getScene()")
	{
		f_getScene();
		return;
	}

	if(message == "getSceneItems()")
	{
		f_getSceneItems();
		return;
	}
}

void MySceneServer::f_getSceneItems()
{
//	myDazExporter
}

void MySceneServer::f_getScene()
{
	dzApp->statusLine("Started exporting scene...",false);

	msgpack::sbuffer sbuf;
	myDazExporter.write(sbuf, 0);

	dzApp->statusLine("Started writing scene to pipe...",false);

	m_socket->write(sbuf.data(),sbuf.size());
	m_socket->flush();

	dzApp->statusLine("Completed sending scene.",false);
}

MySceneServer::~MySceneServer()
{
	if(pBuf){
		UnmapViewOfFile(pBuf);
	}
	if(hMapFile){
		CloseHandle(hMapFile);
	}
}

void 	MySceneServer::executeAction()
{
	dzApp->statusLine("Started writing scene to shared memory...",false);

	MyDazExporter exporter;
	msgpack::sbuffer sbuf;
	exporter.write(sbuf, 0);
	
	memcpy(pBuf,sbuf.data(),sbuf.size());

	dzApp->statusLine("Completed writing scene to shared memory.",false);
}

void 	MySceneServer::toggleAction(bool onOff)
{

}

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