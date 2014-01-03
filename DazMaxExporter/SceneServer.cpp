#include "SceneServer.h"
#include <Winuser.h>
#include <tchar.h>
#include <Windows.h>
#include <Psapi.h>

TCHAR sharedMemoryName[] = TEXT("Local\\DazMaxBridgeSharedMemory");
TCHAR namedPipeName[] = TEXT("DazMaxBridgePipe");

void MySceneServer::startServer()
{
	dazInstanceName = "Daz Instance";

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

	if(message == "getSceneItems()")
	{
		vector<string> items;

		while(m_socket->bytesAvailable())
		{
			string line = m_socket->readLine().trimmed();
			items.push_back(line); 
		}

		f_getScene(items);
		return;
	}

	if(message == "getMySceneInformation()")
	{
		f_getSceneInformation();
		return;
	}

	if(message == "getInstanceName()")
	{
		f_getInstanceName();
		return;
	}
}

void MySceneServer::f_getScene(vector<string> items)
{
	myDazExporter.updateMySceneInformation();

	if(items.size() <= 0)
	{
		items = myDazExporter.sceneInfo.TopLevelItemNames;
	}

	dzApp->statusLine("Started exporting scene...",false);

	msgpack::sbuffer sbuf;
	myDazExporter.write(items, sbuf);

	dzApp->statusLine("Started writing scene to pipe...",false);

	m_socket->write(sbuf.data(),sbuf.size());
	m_socket->flush();

	dzApp->statusLine("Completed sending scene.",false);
}

void MySceneServer::f_getSceneInformation()
{
	myDazExporter.updateMySceneInformation();
	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, myDazExporter.sceneInfo);
	m_socket->write(sbuf.data(), sbuf.size());
	m_socket->flush();
}

void MySceneServer::f_getInstanceName()
{
	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, dazInstanceName);
	m_socket->write(sbuf.data(), sbuf.size());
	m_socket->flush();
}

MySceneServer::~MySceneServer()
{
}

/*
Run this line of DazScript to execute this action from the GUI:
	MainWindow.getActionMgr().findAction("MySceneServer").executeAction()
*/

void 	MySceneServer::executeAction()
{
	ShowMessage(QString::fromStdString("Daz Instance name is: " + dazInstanceName));
}

void 	MySceneServer::toggleAction(bool onOff)
{

}