#include "ConsoleDisplay.h"

ConsoleDisplay::ConsoleDisplay() {}

ConsoleDisplay::~ConsoleDisplay() {}

void ConsoleDisplay::clearConsole()
{
}

void ConsoleDisplay::draw()
{
}

DWORD WINAPI ConsoleDisplay::displayThread()
{
    printf("ConsoleDisplay::displayThread\n");
    fflush(stdout);
    return 0;
}

bool ConsoleDisplay::init()
{
    printf("ConsoleDisplay::init\n");
    fflush(stdout);
    return true;
}

void ConsoleDisplay::run()
{
    DWORD threadId;

    printf("ConsoleDisplay::run\n");
    fflush(stdout);
    hDisplayThread = CreateThread(NULL, 0, &startDisplayThread, (void *)this, 0, &threadId);
}

void ConsoleDisplay::setDisplaySettings(DisplaySettings displaySettings)
{
}