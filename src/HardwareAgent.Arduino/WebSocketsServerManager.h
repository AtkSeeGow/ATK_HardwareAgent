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
    handleDataEnvelope(0, inputValue);
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

          log_d("Client %u connected", num);
          log_d("%s", json.c_str());

          webSocketsServer.sendTXT(num, json);
          break;
        }

      case WStype_DISCONNECTED:
        {
          log_d("Client %u disconnected", num);
          break;
        }

      case WStype_TEXT:
        {
          String msg = String((char*)payload);

          // 要思考這邊如何將原始資料中重要的部分進行保存，讓後續能正確回應
          handleDataEnvelope(num, msg);

          break;
        }
    }
  }

  void broadcastInfo() {
    String json = getInfo();
    log_d("%s", json.c_str());
    webSocketsServer.broadcastTXT(json);
  }

  // 指令解讀
  void handleDataEnvelope(uint8_t clientNum, String payload) {
    log_d("%s", payload.c_str());

    StaticJsonDocument<512> jsonDocument;
    DeserializationError error = deserializeJson(jsonDocument, payload);

    if (error) {
      log_d("deserializeJson() failed: %s", error.c_str());
      return;
    }

    deviceController->handleCommand(jsonDocument);

    StaticJsonDocument<128> newPayload;
    newPayload["status"] = "success";

    String newPayloadStr;
    serializeJson(newPayload, newPayloadStr);

    jsonDocument["payload"] = newPayloadStr;

    jsonDocument["source"] = 1;
    jsonDocument["destination"] = 3;

    String output;
    serializeJson(jsonDocument, output);

    log_d("%s", output.c_str());

    webSocketsServer.sendTXT(clientNum, output);
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