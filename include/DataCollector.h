#ifndef _DATA_COLLECTOR_H_
#define _DATA_COLLECTOR_H_

#include <windows.h>

class DataCollector
{
private:
    HANDLE hDataValidEvent;

    /**
     * @brief       Thread that collects the Race Data from API.
     */
    void dataThread();

public:
    /**
     * @brief       Constructor.
     */
    DataCollector();

    /**
     * @brief       Destructor.
     */
    ~DataCollector();

    /**
     * @brief       Initialization of iRacing API.
     * @return      True if successfull. False if failed.
     */
    bool init();

    /**
     * @brief       Deinitialization of iRacing API.
     */
    void deinit();

    /**
     * @brief       Called from main. Starts Threads.
     */
    void run();
};

#endif /* _DATA_COLLECTOR_H_ */