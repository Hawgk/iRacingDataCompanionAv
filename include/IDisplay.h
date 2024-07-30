#ifndef _DISPLAY_INTERFACE_H_
#define _DISPLAY_INTERFACE_H_

#include <windows.h>

#include "DataModel.h"
#include "Settings.h"

class IDisplay
{
private:
    HANDLE hDisplayThread;
    RaceData raceData;

    virtual DWORD WINAPI displayThread() = 0;
    virtual void draw() = 0;

public:
    static DWORD WINAPI startDisplayThread(void *param)
    {
        IDisplay *self = (IDisplay *)param;
        return self->displayThread();
    }

    virtual ~IDisplay() { }
    virtual bool init() = 0;
    virtual void run() = 0;
    virtual void setDisplaySettings(DisplaySettings displaySettings) = 0;
};

#endif /* _DISPLAY_INTERFACE_H_ */