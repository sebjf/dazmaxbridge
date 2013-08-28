
#include "DazMaxExporter.h"
#include <QtNetwork\qlocalserver.h>
#include <QtNetwork\qlocalsocket.h>


class MySceneServer : public DzAction
{
	Q_OBJECT
public:
	MySceneServer() : DzAction(QString("MaxBridgeAction"),QString("The MaxBridge Action Object"))
	{
		startServer();
	}

public slots:
	virtual void 	executeAction();
	virtual void 	toggleAction (bool onOff);

	void	newConnection();
	void	messageReceived();

private:

	void	startServer();

	~MySceneServer();

	QLocalServer* m_server;
	QLocalSocket* m_socket;

	void	f_getScene();
	void	f_getSceneItems();

	MyDazExporter	myDazExporter;

};