
#include "DazMaxExporter.h"
#include <QtNetwork\qtcpserver.h>
#include <QtNetwork\qtcpsocket.h>

//http://tsenmu.com/blog/?p=344

class MySceneServer : public DzAction
{
	Q_OBJECT
public:
	MySceneServer() : DzAction(QString("MaxBridgeAction"),QString("The MaxBridge Action Object"))
	{
		createSharedMemory(400000000); //400Mb
	}

	void createSharedMemory(DWORD BUF_SIZE);

public slots:
	virtual void 	executeAction();
	virtual void 	toggleAction (bool onOff);

private:
	HANDLE		hMapFile;
	LPVOID		pBuf;

	~MySceneServer();

};