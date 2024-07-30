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

    virtual DWORD WINAPI displayThread();
    virtual void draw();

public:
    static DWORD WINAPI startDisplayThread(void *param)
    {
        IDisplay *self = (IDisplay *)param;
        return self->displayThread();
    }

    virtual ~IDisplay() { }
    virtual bool init();
    virtual void run();
    virtual void setDisplaySettings(DisplaySettings displaySettings);
};

#endif /* _DISPLAY_INTERFACE_H_ */