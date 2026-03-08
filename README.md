# ATK_HardwareAgent

## Introduction

ATK_HardwareAgent 是一個連接
**大型語言模型（LLM）與硬體裝置** 的代理服務（Hardware Agent）。

此專案負責：

-   接收使用者請求
-   與 AnythingLLM 進行溝通
-   解析 AI 回傳的 JSON 指令
-   控制 M5Stick 硬體裝置

透過這個架構，可以讓桌面 AI
助理具備實體互動能力，例如顯示表情、播放語音提醒或進行簡單的情緒互動。

------------------------------------------------------------------------

## Project Goal

本專案的主要目標是建立一個 **LLM → Hardware Control 的橋接層**，讓 AI
可以控制實體裝置。

主要功能包含：

-   將使用者請求轉送給 LLM
-   接收 LLM 回傳的 JSON 指令
-   解析並轉換為硬體控制指令
-   控制 M5Stick 裝置顯示或播放內容

------------------------------------------------------------------------

## System Architecture

    User
      │
      │ Request
      ▼
    ATK_HardwareAgent
      │
      │ LLM Request
      ▼
    AnythingLLM (Desktop AI Assistant)
      │
      │ JSON Response
      ▼
    ATK_HardwareAgent
      │
      │ JSON Command
      ▼
    M5Stick Device
      │
      ├─ LCD Display
      ├─ LED Indicator
      └─ Speaker (TTS Audio)

------------------------------------------------------------------------

## Component Description

### ATK_HardwareAgent

核心代理服務，負責：

-   接收使用者請求
-   呼叫 AnythingLLM API
-   解析 LLM 回傳 JSON
-   將指令轉送給硬體裝置

### AnythingLLM

AnythingLLM 作為 AI 桌面助理，負責：

-   理解使用者需求
-   產生硬體控制 JSON 指令
-   進行簡單決策與提醒

### M5Stick Device

M5Stick 是實體硬體裝置，負責：

-   接收 JSON 指令
-   控制 LCD / LED
-   播放語音

------------------------------------------------------------------------

## Data Flow

1.  使用者透過 Agent 發送請求
2.  Agent 呼叫 AnythingLLM
3.  LLM 產生 JSON 控制指令
4.  Agent 解析 JSON
5.  Agent 將指令傳送給 M5Stick
6.  M5Stick 解析 JSON 並控制裝置

------------------------------------------------------------------------

## Example JSON Command

### 顯示表情

``` json
{
  "action": "display_face",
  "face": "happy"
}
```

### LED 控制

``` json
{
  "action": "set_led",
  "color": "green"
}
```

### 播放語音

``` json
{
  "action": "play_tts",
  "text": "記得起來活動一下喔"
}
```

------------------------------------------------------------------------

## Example Use Cases

### 工作提醒

AnythingLLM 可以定時提醒使用者：

-   LCD 顯示提醒表情
-   播放語音提醒

例如： 「嘿～工作一段時間了，起來活動一下吧。」

### 情緒互動

AI 可以透過硬體裝置呈現情緒：

  狀態   LCD 表情   LED
  ------ ---------- ------
  開心   😊         綠色
  提醒   😐         黃色
  警告   ⚠️         紅色

------------------------------------------------------------------------

## Future Extensions

未來可以擴充：

-   語音輸入（Speech → LLM）
-   IoT 裝置控制
-   更多硬體設備整合
-   AI 情緒互動系統
-   智慧桌面助手
