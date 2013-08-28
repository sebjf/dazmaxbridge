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

}

void 	MySceneServer::executeAction()
{
	ShowMessage("MySceneServer action executed.");
}

void 	MySceneServer::toggleAction(bool onOff)
{

}