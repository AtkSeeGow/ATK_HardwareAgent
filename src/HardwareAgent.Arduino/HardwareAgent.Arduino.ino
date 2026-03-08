#include <M5Unified.h>
#include <HTTPClient.h>
#include "Config.h"
#include "WifiManager.h"

WifiManager wifiManager;

unsigned long lastPoll = 0;
const int pollInterval = 3000;

void setup() {
  auto cfg = M5.config();
  M5.begin(cfg);

  Serial.begin(115200);

  M5.Display.setTextSize(3);
  M5.Display.setRotation(3);
  M5.Display.println("AI Hardware Agent");

  wifiManager.connect();
}

void loop() {
  if (millis() - lastPoll > pollInterval) {
    lastPoll = millis();

    HTTPClient http;
    http.begin(HEARTBEAT_API);
    int httpCode = http.GET();
    if (httpCode == 200) {
      String payload = http.getString();
      Serial.println(payload);
    }

  }
}