#ifndef _SETTINGS_READER_H_
#define _SETTINGS_READER_H_

#include "Settings.h"

/**
 * @brief       Read Display Settings section from settings file.
 * @return      DisplaySettings struct containing data from file.
 */
DisplaySettings readDisplaySettings();

/**
 * @brief       Read Data Collector Settings section from settings file.
 * @return      CollectorSettings struct containing data from file.
 */
CollectorSettings readCollectorSettings();

/**
 * @brief       Write Display Settings section back to settings file.
 * @param      displaySettings      Struct containing data that shall be written to file.
 */
void writeDisplaySettings(DisplaySettings displaySettings);
/**
 * @brief       Write Data Collector Settings section back to settings file.
 * @param      collectorSettings    Struct containing data that shall be written to file.
 */
void writeCollectorSettings(CollectorSettings collectorSettings);

#endif /* _SETTINGS_READER_H_ */