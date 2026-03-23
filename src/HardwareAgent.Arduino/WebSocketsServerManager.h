#pragma once

#include <ArduinoJson.h>
#include <WebSocketsServer.h>
#include "config.h"

class WebSocketsServerManager {
public:
  void init() {
    instance = this;
    webSocketsServer.begin();
    webSocketsServer.onEvent(eventWrapper);
  }

  void loop() {
    webSocketsServer.loop();
  }

  void handleSerialInput(String inputValue) {
    if (inputValue == "LED ON") {
      broadcastInfo();
    } else if (inputValue == "LED OFF") {
      broadcastInfo();
    } else {
      broadcastSerialMessage(inputValue);
    }
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
        Serial.printf("Client %u connected\n", num);
        webSocketsServer.sendTXT(clientNum, getInfo());
        break;

      case WStype_DISCONNECTED:
        Serial.printf("Client %u disconnected\n", num);
        break;

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

  void handleCommand(uint8_t clientNum, String payload) {
    StaticJsonDocument<200> doc;
    DeserializationError error = deserializeJson(doc, payload);

    if (error) {
      Serial.println("JSON parse failed");
      return;
    }

    String type = doc["type"];

    if (type == "serial") {
      String msg = doc["message"];
      Serial.println("[WS RECV] " + msg);
    }
  }

  void broadcastSerialMessage(String message) {
    StaticJsonDocument<200> doc;
    doc["type"] = "serial";
    doc["message"] = message;

    String json;
    serializeJson(doc, json);

    webSocketsServer.broadcastTXT(json);
  }

  String getInfo() {
    StaticJsonDocument<200> doc;
    doc["type"] = "state";

    // 準備擴充這一段，由各個控制物件取得對應資訊
    // 於連線或有一方進行更改時，將資料打出去

    String json;
    serializeJson(doc, json);
    return json;
  }
};