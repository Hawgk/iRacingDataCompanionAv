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
    SetPriorityClass(GetCurrentProcess(), HIGH_PRIORITY_CLASS);
    timeBeginPeriod(1);
    hDataValidEvent = CreateEvent(NULL, true, false, IRSDK_DATAVALIDEVENTNAME);

    return true;
}

void DataCollector::deinit()
{
    printf("DataCollector::deinit\n");
    if (hDataValidEvent)
    {
        ResetEvent(hDataValidEvent);
        CloseHandle(hDataValidEvent);
        hDataValidEvent = NULL;
    }

    timeEndPeriod(1);
}

DWORD WINAPI DataCollector::dataThread()
{
    bool wasUpdated = false;

    printf("DataCollector::dataThread\n");
    while (irsdkClient::instance().isConnected())
    {
        if (irsdkClient::instance().waitForData(16))
        {
            /*
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
            */
        }
    }

    return 0;
}

void DataCollector::run()
{
    DWORD threadId;

    printf("DataCollector::run\n");
    hDataThread = CreateThread(NULL, 0, &startDataThread, (void *)this, 0, &threadId);
    SetPriorityClass(hDataThread, HIGH_PRIORITY_CLASS);
}