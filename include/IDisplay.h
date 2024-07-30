#ifndef _DISPLAY_INTERFACE_H_
#define _DISPLAY_INTERFACE_H_

#include "DataModel.h"
#include "Settings.h"

class IDisplay
{
private:
    RaceData raceData;
    virtual void displayThread();
    virtual void draw();

public:
    virtual ~IDisplay() { }
    virtual bool init();
    virtual void setDisplaySettings(DisplaySettings displaySettings);
};

#endif /* _DISPLAY_INTERFACE_H_ */