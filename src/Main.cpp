#include <Windows.h>
#include <stdio.h>
#include <time.h>

#include "irsdk_defines.h"
#include "irsdk_client.h"

#include "DataCollector.h"
#include "ConsoleDisplay.h"

#define NO_STDIO_REDIRECT

int main()
{
    DataCollector *dataCollector = new DataCollector();
    ConsoleDisplay *consoleDisplay = new ConsoleDisplay();

    if (dataCollector->init() && consoleDisplay->init())
    {
        printf("Waiting for iRacing connection\n");
        fflush(stdout);
        
        dataCollector->run();
        consoleDisplay->run();
    }

    return 0;
}