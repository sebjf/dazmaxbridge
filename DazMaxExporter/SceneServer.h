
#include "DazMaxExporter.h"
#include <QtNetwork\qtcpserver.h>
#include <QtNetwork\qtcpsocket.h>

//http://tsenmu.com/blog/?p=344

class MySceneServer : public QObject
{
	Q_OBJECT
public:
	MySceneServer(QObject* parent = 0) : QObject(parent) 
	{
		server = new QTcpServer(this);
		connect(server, SIGNAL(newConnection()),
		this, SLOT(on_newConnection()));
	}

	void Listen();

public slots:
	void on_newConnection();
	void on_readyRead();
	void on_disconnected();

private:
	QTcpServer* server;
	QTcpSocket* socket;
};