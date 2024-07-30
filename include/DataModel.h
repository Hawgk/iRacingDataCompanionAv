#ifndef _DATA_MODEL_H_
#define _DATA_MODEL_H_

#include <stdint.h>
#include "irsdk_defines.h"

typedef struct color_argb_s
{
    uint8_t alpha;
    uint8_t red;
    uint8_t green;
    uint8_t blue;
} ColorArgb;

typedef struct tb_trace_s
{
    bool show;

    float brake;            /*< Brake */
    float throttle;         /*< Throttle */
    float clutch;           /*< Clutch */

    ColorArgb colorBrake;
    ColorArgb colorThrottle;
    ColorArgb colorClutch;
} ThBrTrace;

typedef struct fuel_data_s
{
    bool show;

    float fuelLevel;        /*< FuelLevel */
    float fuelPerLapAvg;
    float fuelPerLapMin;
    float fuelPerLapMax;
} FuelData;

typedef struct car_lap_data_s
{
    int32_t id;             /*< CarIdx */
    bool isPlayer;          /*< Check against PlayerCarClassPosition? */
    int32_t classPos;       /*< CarIdxClassPosition */
    float relativeDiff;     /*< CarIdxEstTime */
    float absoluteDiff;     /*< CarIdxF2Time */
    int32_t lap;            /*< CarIdxLap */
    bool inPit;             /*< CarIdxOnPitRoad */
    irsdk_TrkLoc trackLoc;  /*< CarIdxTrackSurface */
} CarLapData;

typedef struct race_data_s
{
    bool show;

    irsdk_Flags flag;       /*< SessionFlags */
    uint32_t raceLapCount;  /*< Lap */
    uint32_t raceLapLeft;   /*< SessionLapsRemain */

    float sessionTimeLeft;  /*< SessionTimeRemain */

    float bestLapTime;      /*< LapBestLapTime */
    float lastLapTime;      /*< LapLastLapTime */

    CarLapData carLapData[64];
} RaceData;

typedef struct weather_data_s
{
    bool show;

    float ambientTemp;      /*< AirTemp */
    float trackTemp;        /*< TrackTemp */
    float fogLevel;         /*< FogLevel */

} WeatherData;

#endif /* _DATA_MODEL_H_ */