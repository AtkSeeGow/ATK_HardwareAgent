#pragma once
#include <M5Unified.h>
#include "time.h"

class DisplayManager {
public:
  unsigned long interval = 1000;
  unsigned long previousMillis = 0;

  void init() {
    M5.Display.fillScreen(BLACK);
    M5.Display.setTextColor(WHITE);
    M5.Display.setRotation(2);
    M5.Display.setBrightness(64);
    M5.Display.setTextDatum(top_left);
  }

  void loop() {
    unsigned long now = millis();
    if (now - previousMillis >= interval) {
      previousMillis = now;

      struct tm timeinfo;
      if (!getLocalTime(&timeinfo)) {
        M5.Display.fillScreen(BLACK);
        M5.Display.drawString("Time Error", M5.Display.width() / 2, M5.Display.height() / 2);
        return;
      }

      M5.Display.fillRect(0, 0, 135, 125, BLACK);

      char datetimeStr[32];

      strftime(datetimeStr, sizeof(datetimeStr), "%Y-%m-%d", &timeinfo);
      M5.Display.setTextFont(4);
      M5.Display.drawString(datetimeStr, 5, 5);

      strftime(datetimeStr, sizeof(datetimeStr), "%H:%M", &timeinfo);
      M5.Display.setTextFont(6);
      M5.Display.drawString(datetimeStr, 5, 30);

      strftime(datetimeStr, sizeof(datetimeStr), "%S", &timeinfo);
      M5.Display.setTextFont(6);
      M5.Display.drawString(datetimeStr, M5.Display.width() - 60, 75);
    }
  }

private:
  
};
