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

  void handleCommand(JsonDocument& jsonDocument) {
    // 1. 取得 payload 物件（ArduinoJson 6/7 推薦寫法）
    JsonObject payload = jsonDocument["payload"]; 

    // 2. 檢查有效性：如果 payload 不是物件或不含 action，就直接結束
    if (payload.isNull() || !payload.containsKey("action")) {
        log_w("無效的指令格式：缺少 payload 或 action");
        return;
    }

    // 3. 安全地取得值 (假設 ledState 是 bool 或 int)
    // as<T>() 可以確保型別轉換安全，如果轉換失敗會回傳預設值
    ledState = payload["action"] == "turn_on";

    log_i("LED 狀態更新為: %s", ledState ? "ON" : "OFF");
  }

private:
  bool ledState = false;
  const int ledPin = 10;
};