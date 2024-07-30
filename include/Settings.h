#ifndef _SETTINGS_H_
#define _SETTINGS_H_

#include <stdint.h>
#include <string>

using namespace std;

typedef struct display_settings_s
{
    uint32_t height;
    uint32_t width;
    uint32_t refreshRate;
    bool visibility;
} DisplaySettings;

typedef struct collector_settings_s
{

} CollectorSettings;

class Settings
{
public:
    /**
     * @brief       Singleton instance function.
     * @return      Object reference of self.
     */
    Settings& instance();

    /**
     * @brief       Set path of settings file.
     * @param       path                Path to settings file.
     */
    void setSettingsPath(std::string path);

    /**
     * @brief       Set Display Settings of settings file.
     * @param       displaySettings     Display settings struct.
     */
    void setDisplaySettings(DisplaySettings displaySettings);

    /**
     * @brief       Set Collector Settings of settings file.
     * @param       displaySettings     Collector settings struct.
     */
    void setCollectorSettings(CollectorSettings collectorSettings);

    /**
     * @brief       Get Display Settings from settings file.
     * @return      Display settings struct.
     */
    DisplaySettings getDisplaySettings();

    /**
     * @brief       Get Collector Settings from settings file.
     * @return      Collector settings struct.
     */
    CollectorSettings getCollectorSettings();
};

#endif /* _SETTINGS_H_ */