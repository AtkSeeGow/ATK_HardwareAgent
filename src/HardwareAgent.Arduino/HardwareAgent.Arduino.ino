#include <M5Unified.h>
#include "time.h"
#include "DisplayManager.h"
#include "WebSocketsServerManager.h"

DisplayManager displayManager;
WebSocketsServerManager webSocketsServerManager;

void setup() {
  Serial.begin(115200);

  auto cfg = M5.config();
  M5.begin(cfg);

  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
  }

  configTime(8 * 3600, 0, "pool.ntp.org");

  webSocketsServerManager.init();
  displayManager.init();
}

String serialBuffer = "";
void loop() {
  webSocketsServerManager.loop();
  displayManager.loop();

  while (Serial.available()) {
    char c = Serial.read();
    if (c == '\n') {
      serialBuffer.trim();
      if (serialBuffer.length() > 0) {
        webSocketsServerManager.handleSerialInput(serialBuffer);
      }
      serialBuffer = "";
    } else {
      serialBuffer += c;
    }
  }
}