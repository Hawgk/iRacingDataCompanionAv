#include <Windows.h>
#include <stdio.h>
#include <time.h>

#include "irsdk_defines.h"
#include "irsdk_client.h"

HANDLE hDataValidEvent = NULL;

static bool init()
{
    SetPriorityClass(GetCurrentProcess(), HIGH_PRIORITY_CLASS);
    timeBeginPeriod(1);
    hDataValidEvent = CreateEvent(NULL, true, false, IRSDK_DATAVALIDEVENTNAME);

    return true;
}

static void deInit()
{
    if (hDataValidEvent)
    {
        // make sure event not left triggered (probably redundant)
        ResetEvent(hDataValidEvent);
        CloseHandle(hDataValidEvent);
        hDataValidEvent = NULL;
    }

    timeEndPeriod(1);
}

int main(int argc, char *argv[])
{
    if (init())
    {
        run();
        deInit();
    }
    else
    {
        /* Init failed */
    }

    return 0;
}