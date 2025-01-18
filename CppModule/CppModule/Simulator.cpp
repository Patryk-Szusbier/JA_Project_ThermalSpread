#include "pch.h" 
#include <utility>
#include <limits.h>
#include "Simulator.h"
#include <debugapi.h>
#include <string>
#include <sstream>

bool _runSingleStep(uint8_t* readData, uint8_t* writeData, int width, int height, int startColumn, int endColumn, uint16_t mul, int32_t shift) {
	bool IsFinished = true;

	for (int row = 1; row <= height; row++) {
		readData += width;
		writeData += width;

		for (int col = startColumn; col <= endColumn; col++) {
			readData += col;
			writeData += col;

			uint8_t centerValue = readData[0];
			uint8_t leftValue = readData[-1];
			uint8_t rightValue = readData[1];
			uint8_t topValue = readData[-width];
			uint8_t bottomValue = readData[width];

			uint8_t newValue = ((centerValue + leftValue + rightValue + topValue + bottomValue) * mul) >> shift;

			*writeData = newValue;

			if (newValue > 0) {
				IsFinished = false;
			}

			readData -= col;
			writeData -= col;
		}
	}

	return IsFinished;
}