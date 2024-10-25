#include <windows.h>
#include <stdio.h>
#include <time.h>

#include "irsdk_defines.h"
#include "irsdk_client.h"

#include "DataCollector.h"

DataCollector::DataCollector() {}

DataCollector::~DataCollector() {}

bool DataCollector::init()
{
    printf("DataCollector::init\n");
    fflush(stdout);
    SetPriorityClass(GetCurrentProcess(), HIGH_PRIORITY_CLASS);
    timeBeginPeriod(1);
    hDataValidEvent = CreateEvent(NULL, true, false, IRSDK_DATAVALIDEVENTNAME);

    return true;
}

void DataCollector::deinit()
{
    printf("DataCollector::deinit\n");
    fflush(stdout);
    if (hDataValidEvent)
    {
        ResetEvent(hDataValidEvent);
        CloseHandle(hDataValidEvent);
        hDataValidEvent = NULL;
    }

    timeEndPeriod(1);
}

void monitorConnectionStatus()
{
	// keep track of connection status
	static bool wasConnected = false;

	bool isConnected = irsdkClient::instance().isConnected();
	if(wasConnected != isConnected)
	{
		if(isConnected)
		{
			printf("Connected to iRacing\n");
		}
		else
			printf("Lost connection to iRacing\n");

		//****Note, put your connection handling here

		wasConnected = isConnected;
	}
}

DWORD WINAPI DataCollector::dataThread()
{
    bool wasUpdated = false;

    printf("DataCollector::dataThread\n");
    fflush(stdout);
    while (true)
    {
        if (irsdkClient::instance().waitForData(0))
        {
            // and grab the data
            processLapInfo();

            if (processYAMLLiveString())
            {
                wasUpdated = true;
            }

            // only process session string if it changed
            if (irsdkClient::instance().wasSessionStrUpdated())
            {
                processYAMLSessionString(irsdkClient::instance().getSessionStr());
                wasUpdated = true;
            }

            // notify clients
            if (wasUpdated && hDataValidEvent)
            {
                PulseEvent(hDataValidEvent);
            }

            updateDisplay();
        }
        monitorConnectionStatus();
    }

    return 0;
}

void DataCollector::run()
{
    DWORD threadId;

    printf("DataCollector::run\n");
    fflush(stdout);
    hDataThread = CreateThread(NULL, 0, &startDataThread, (void *)this, 0, &threadId);
    SetPriorityClass(hDataThread, HIGH_PRIORITY_CLASS);
}