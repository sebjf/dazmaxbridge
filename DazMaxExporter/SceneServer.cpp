#include "SceneServer.h"
#include <Winuser.h>
#include <tchar.h>
#include <Windows.h>
#include <Psapi.h>

TCHAR namedPipeName[] = TEXT("DazMaxBridgePipe");

int CountOtherPipes()
{
	WIN32_FIND_DATA FindFileData;
    HANDLE hFind;
	memset(&FindFileData, 0, sizeof(FindFileData));

	const char* filter = "//./pipe/*";

	hFind = FindFirstFileA(filter, &FindFileData);

	int count = 0;

	if (hFind != INVALID_HANDLE_VALUE)
	{
		do
        {
			if(strstr(FindFileData.cFileName, namedPipeName) != NULL){
				count++;
			}
        }
        while (FindNextFile(hFind, &FindFileData));

		FindClose(hFind);
	}
	
	return count;
}

void MySceneServer::startServer()
{
	int instanceNumber = CountOtherPipes() + 1;
	dazInstanceName = QString("Daz Instance #%1").arg(QString::number(instanceNumber)).toLocal8Bit().data();

	QString uniquePipeName = QString(namedPipeName) + "_" + QUuid::createUuid().toString();

	m_server = new QLocalServer(this);
	if(!m_server->listen(uniquePipeName)){
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

	myDazExporter.startProfiling();

	myDazExporter.write(items, sbuf);

	myDazExporter.endProfiling("packed scene in");

	myDazExporter.startProfiling();

	m_socket->write(sbuf.sharedMemory.name + "\n");
	m_socket->flush();

	/* This resets the position so the buffer can be reused with the existing allocated memory, assuming that Daz and Max will negotiate access */
	sbuf.clear();

	myDazExporter.endProfiling("wrote scene in");

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
	string message = "Daz Instance name is: " + dazInstanceName;
	ShowMessage(message.c_str());
}

void 	MySceneServer::toggleAction(bool onOff)
{

}