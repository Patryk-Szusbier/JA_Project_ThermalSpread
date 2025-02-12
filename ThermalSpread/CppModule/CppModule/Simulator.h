#pragma once

#define SIMULATOR_API __declspec(dllimport)

extern "C" SIMULATOR_API bool _runSingleStep(uint8_t* readData, uint8_t* writeData, int width, int height, int startColumn, int endColumn, uint16_t mul, int32_t shift);