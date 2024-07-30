cmake_minimum_required(VERSION 3.30.1)
set(CMAKE_C_COMPILER gcc.exe)
set(CMAKE_CXX_COMPILER g++.exe)

project(iStopping)

# Program sources
file(GLOB_RECURSE SOURCES
    src/*.cpp
)

# Library sources
file(GLOB_RECURSE LIB_SOURCES
    lib/irsdk/irsdk_client.cpp
    lib/irsdk/irsdk_diskclient.cpp
    lib/irsdk/irsdk_utils.cpp
    lib/irsdk/yaml_parser.cpp
)

# Resource data that will be copied to build directory
file(GLOB_RECURSE DATA
    resources/*
)

add_executable(iStopping 
    ${SOURCES}
    ${LIB_SOURCES}
)

target_compile_options(iStopping PUBLIC 
    -std=c++17
    -W4
)

target_include_directories(iStopping PUBLIC 
    include
    lib/irsdk
)

# Copy resources to build directory
#file(COPY ${DATA} DESTINATION resources)

# Add precompiled libraries
#target_link_libraries(example PUBLIC ...)
