#include "SceneServer.h"

MyDazUtility::MyDazUtility()
{
	mySceneServer = new MySceneServer(this);
	mySceneServer->Listen();
}

void MySceneServer::Listen()
{
	server->listen(QHostAddress::LocalHost, 12121);
}

void MySceneServer::on_newConnection()
{
	socket = server->nextPendingConnection();
	socket->setTextModeEnabled(false);
	if(socket->state() == QTcpSocket::ConnectedState)
	{
		printf("New connection established.\n");
	}
	connect(socket, SIGNAL(disconnected()),	this, SLOT(on_disconnected()));
	connect(socket, SIGNAL(readyRead()), this, SLOT(on_readyRead()));
}

void MySceneServer::on_readyRead()
{
	while(socket->canReadLine())
	{
		QByteArray ba = socket->readLine();

		if(strncmp(ba.constData(),"getScene",strlen("getScene")) == 0)
		{
			MyDazExporter exporter;
			exporter.write(*socket, (DzFileIOSettings*)0);
			socket->flush();
			continue;
		}

		printf("Unkown command: %s", ba.constData());
	}
}

void MySceneServer::on_disconnected()
{
	printf("Connection disconnected.\n");
	disconnect(socket, SIGNAL(disconnected()));
	disconnect(socket, SIGNAL(readyRead()));
	socket->deleteLater();
}