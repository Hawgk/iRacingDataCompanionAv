#include <Windows.h>
#include <stdio.h>
#include <time.h>

#include "irsdk_defines.h"
#include "irsdk_client.h"

#include "DataCollector.h"
#include "ConsoleDisplay.h"

int main()
{
    DataCollector *dataCollector = new DataCollector();
    ConsoleDisplay *consoleDisplay = new ConsoleDisplay();

    if (dataCollector->init() && consoleDisplay->init())
    {
        printf("Waiting for iRacing connection\n");
        while (!irsdkClient::instance().isConnected()) {}
        
        printf("iRacing connected\n");
        dataCollector->run();
        consoleDisplay->run();
    }

    return 0;
}