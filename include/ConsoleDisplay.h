#ifndef _CONSOLE_DISPLAY_H_
#define _CONSOLE_DISPLAY_H_

#include "IDisplay.h"

class ConsoleDisplay : IDisplay
{
private:
    RaceData raceData;

    /**
     * @brief       Clear all console outputs.
     */
    void clearConsole();

    /**
     * @brief       Draw current race data.
     * Called by displayThread.
     * @note        Implements interface.
     */
    virtual void draw();

    /**
     * @brief       Thread that displays the Race Data to console.
     * @note        Implements interface.
     */
    virtual void displayThread();

public:
    /**
     * @brief       Initialize console output.
     * @note        Implements interface.
     */
    virtual bool init();

    /**
     * @brief       Set Display Settings for console output.
     * @note        Implements interface.
     * @param       displaySettings     Struct containing the display settings.
     */
    virtual void setDisplaySettings(DisplaySettings displaySettings);
};

#endif /* _CONSOLE_DISPLAY_H_ */