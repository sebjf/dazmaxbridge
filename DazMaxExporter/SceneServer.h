
#include "DazMaxExporter.h"
#include <QtNetwork\qlocalserver.h>
#include <QtNetwork\qlocalsocket.h>

class MySceneServer : public DzAction
{
	Q_OBJECT
public:
	MySceneServer() : DzAction(QString("MaxBridgeAction"),QString("The MaxBridge Action Object"))
	{
		LARGE_INTEGER frequency;
		QueryPerformanceFrequency(&frequency);
		performanceCounterFrequency = double(frequency.QuadPart);

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

	void	f_getScene(RequestParameters params);
	void	f_getSceneInformation();
	void	f_getInstanceName();

	MyDazExporter	myDazExporter;

	string	dazInstanceName;

	double performanceCounterFrequency;

	msgpack::sharedmembuffer sbuf;

};