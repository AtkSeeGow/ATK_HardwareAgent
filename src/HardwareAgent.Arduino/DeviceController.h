#pragma once

#include <Arduino.h>
#include <ArduinoJson.h>

class DeviceController {
public:
  void init() {
    pinMode(ledPin, OUTPUT);
  }

  void loop() {
    digitalWrite(ledPin, !ledState);
  }

  void getInfo(JsonDocument& doc) {
    doc["led"] = ledState;
  }

  void handleCommand(JsonDocument& doc) {
    if (doc.containsKey("led")) {
      ledState = doc["led"];
    }
  }

private:
  bool ledState = false;
  const int ledPin = 10;
};