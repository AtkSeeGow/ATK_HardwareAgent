#include "WifiManager.h"
#include <WiFi.h>
#include "config.h"

void WifiManager::connect()
{
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
    }
}