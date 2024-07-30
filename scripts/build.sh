#!/bin/sh
cd ..

if [ -d "build" ]; then
    rm -rf build
fi

mkdir build
cd build

cmake -G "Visual Studio 17 2022" -S .. -B .
cmake --build .