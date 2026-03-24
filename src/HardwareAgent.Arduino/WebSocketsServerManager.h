#pragma once

#include <ArduinoJson.h>
#include <WebSocketsServer.h>
#include "config.h"
#include "DeviceController.h"

class WebSocketsServerManager {
public:

  DeviceController* deviceController = nullptr;

  void init(DeviceController* dc) {
    instance = this;
    deviceController = dc;
    webSocketsServer.begin();
    webSocketsServer.onEvent(eventWrapper);
  }

  void loop() {
    webSocketsServer.loop();
  }

  void handleSerialInput(String inputValue) {
    handleCommand(0, inputValue);
    broadcastInfo();
  }

private:
  // ⭐ 核心：保存 instance
  inline static WebSocketsServerManager* instance = nullptr;

  WebSocketsServer webSocketsServer{ WEB_SOCKETS_SERVER_PORT };

  // ⭐ wrapper（給 WebSocket）
  static void eventWrapper(uint8_t num, WStype_t type, uint8_t* payload, size_t length) {
    if (instance) {
      instance->event(num, type, payload, length);
    }
  }

  // ⭐ 真正邏輯（非 static）
  void event(uint8_t num, WStype_t type, uint8_t* payload, size_t length) {
    switch (type) {
      case WStype_CONNECTED:
        {
          String json = getInfo();
          Serial.printf("Client %u connected\n", num);
          Serial.println(json);
          webSocketsServer.sendTXT(num, json);
          break;
        }

      case WStype_DISCONNECTED:
        {
          Serial.printf("Client %u disconnected\n", num);
          break;
        }

      case WStype_TEXT:
        {
          String msg = String((char*)payload);
          Serial.println("[WS RECV RAW] " + msg);
          handleCommand(num, msg);
          break;
        }
    }
  }

  void broadcastInfo() {
    String json = getInfo();
    webSocketsServer.broadcastTXT(json);
    Serial.println("[WS SEND] " + json);
  }

  // 指令解讀
  void handleCommand(uint8_t clientNum, String payload) {
    StaticJsonDocument<200> doc;
    DeserializationError error = deserializeJson(doc, payload);

    if (error) {
      Serial.println("JSON parse failed");
      return;
    }

    deviceController->handleCommand(doc);
  }

  // 取得裝置資訊
  String getInfo() {
    StaticJsonDocument<200> doc;
    doc["type"] = "info";
    deviceController->getInfo(doc);
    String json;
    serializeJson(doc, json);
    return json;
  }
};