# Server-Side SDUI Requirements

**Client Version**: SVN r139  
**Document Version**: 1.0  
**Date**: 2026-02-17

## Overview

This document specifies the server-side changes required to support the client's migration to a Server-Driven UI (SDUI) architecture. The goal is to minimize client-side localization, keeping only 5 technically essential keys on the client, while all other UI texts are pushed from the server.

---

## 1. Protocol Definitions

### 1.1 StartSettingsUI Protocol

**Purpose**: Push all UI texts for the StartSettings interface

**Definition** (Protobuf):

```protobuf
message StartSettingsUI {
    // Section titles
    string accounts = 1;
    string general = 2;
    
    // Account management
    string add_account = 3;
    string edit = 4;
    string delete = 5;
    string edit_account = 6;
    string account_id = 7;
    string password = 8;
    string note_optional = 9;
    string delete_confirm = 10;
    
    // Settings items
    string language = 11;
    string font_size = 12;
    string ui_sound = 13;
    
    // Font size options
    string font_size_small = 14;
    string font_size_medium = 15;
    string font_size_large = 16;
    string font_size_extra_large = 17;
    
    // Dialog buttons
    string confirm = 18;
    string cancel = 19;
    
    // Language names (22 languages)
    map<string, string> language_names = 20;  // key: "ChineseSimplified", value: "简体中文"
}
```

**Language Names Mapping**:
```
ChineseSimplified → 简体中文
ChineseTraditional → 繁體中文
English → English
Japanese → 日本語
Korean → 한국어
French → Français
German → Deutsch
Spanish → Español
Italian → Italiano
Portuguese → Português
Russian → Русский
Arabic → العربية
Turkish → Türkçe
Polish → Polski
Dutch → Nederlands
Swedish → Svenska
Norwegian → Norsk
Danish → Dansk
Finnish → Suomi
Greek → Ελληνικά
Czech → Čeština
Hungarian → Magyar
```

---

### 1.2 ErrorTexts Protocol

**Purpose**: Push all network error texts to the client (used after login for runtime error display)

**Definition** (Protobuf):

```protobuf
message ErrorTexts {
    string connection_failed = 1;
    string connection_refused = 2;
    string server_disconnected = 3;
    string network_communication_error = 4;
    string send_failed = 5;
    string rate_limit = 6;
    string reconnecting_countdown = 7;  // Format: "{1} seconds until reconnect (attempt {0})..."
    string reconnecting_attempt = 8;    // Format: "Reconnecting (attempt {0})..."
    string reconnect_success = 9;
    string reconnect_cancel = 10;
}
```

**Note**: The `reconnecting_countdown` and `reconnecting_attempt` texts use string format placeholders:
- `{0}` = attempt number (e.g., 1, 2, 3)
- `{1}` = seconds remaining (for countdown only)

**Example** (Japanese):
```
reconnecting_countdown: "{1}秒後に再接続（第{0}回）..."
reconnecting_attempt: "再接続中（第{0}回）..."
```

---

### 1.3 ChangeLanguage Protocol

**Purpose**: Allow the client to request UI texts in a different language after login

**Definition** (Protobuf):

```protobuf
message ChangeLanguageRequest {
    string language = 1;  // "Japanese", "Korean", "ChineseSimplified", etc.
}

message ChangeLanguageResponse {
    bool success = 1;
    StartSettingsUI start_settings_ui = 2;  // New language texts for StartSettings
    ErrorTexts error_texts = 3;             // New language texts for errors
    // Future: add other UI texts as needed (Home, Option, etc.)
}
```

**Client Behavior**:
- When user clicks "Language" item in StartSettings, client cycles to next language
- Client sends `ChangeLanguageRequest` with new language
- Client receives `ChangeLanguageResponse` with updated UI texts
- Client stores updated texts in `Data.Instance`
- Client rebuilds UI to reflect new language

---

## 2. Modified Existing Protocols

### 2.1 GatewayResponse

**Current**:
```protobuf
message GatewayResponse {
    repeated ServerInfo servers = 1;
    UIConfig ui = 2;
}
```

**Add**:
```protobuf
message GatewayResponse {
    repeated ServerInfo servers = 1;
    UIConfig ui = 2;
    StartSettingsUI start_settings_ui = 3;  // NEW
}
```

**Implementation**:
```typescript
function handleGatewayRequest(language: string): GatewayResponse {
    return {
        servers: getServerList(),
        ui: getStartUI(language),
        start_settings_ui: getStartSettingsUI(language)  // Push StartSettings texts
    };
}
```

---

### 2.2 LoginResponse

**Add**:
```protobuf
message LoginResponse {
    // ... existing fields ...
    ErrorTexts error_texts = 100;  // NEW
}
```

**Implementation**:
```typescript
function handleLogin(request: LoginRequest): LoginResponse {
    // Authenticate user...
    
    return {
        success: true,
        // ... other fields ...
        error_texts: getErrorTexts(request.language)  // Push error texts
    };
}
```

---

## 3. Server-Side Localization Management

### 3.1 File Structure

**Create localization resource files** organized by language:

```
server/resources/localization/
├── zh-CN.json  (Simplified Chinese)
├── zh-TW.json  (Traditional Chinese)
├── en.json     (English)
├── ja.json     (Japanese)
├── ko.json     (Korean)
├── fr.json     (French)
├── de.json     (German)
├── es.json     (Spanish)
├── it.json     (Italian)
├── pt.json     (Portuguese)
├── ru.json     (Russian)
├── ar.json     (Arabic)
├── tr.json     (Turkish)
├── pl.json     (Polish)
├── nl.json     (Dutch)
├── sv.json     (Swedish)
├── no.json     (Norwegian)
├── da.json     (Danish)
├── fi.json     (Finnish)
├── el.json     (Greek)
├── cs.json     (Czech)
└── hu.json     (Hungarian)
```

---

### 3.2 File Content Template

**Example** (`ja.json`):

```json
{
  "startSettings": {
    "accounts": "アカウント",
    "general": "一般設定",
    "add_account": "アカウント追加",
    "edit": "編集",
    "delete": "削除",
    "edit_account": "アカウント編集",
    "account_id": "アカウントID",
    "password": "パスワード",
    "note_optional": "備考（任意）",
    "delete_confirm": "このアカウントを削除してもよろしいですか？",
    "language": "言語",
    "font_size": "フォントサイズ",
    "ui_sound": "サウンド",
    "font_size_small": "小",
    "font_size_medium": "中",
    "font_size_large": "大",
    "font_size_extra_large": "特大",
    "confirm": "確認",
    "cancel": "キャンセル",
    "languageNames": {
      "ChineseSimplified": "簡体字中国語",
      "ChineseTraditional": "繁体字中国語",
      "English": "英語",
      "Japanese": "日本語",
      "Korean": "韓国語",
      "French": "フランス語",
      "German": "ドイツ語",
      "Spanish": "スペイン語",
      "Italian": "イタリア語",
      "Portuguese": "ポルトガル語",
      "Russian": "ロシア語",
      "Arabic": "アラビア語",
      "Turkish": "トルコ語",
      "Polish": "ポーランド語",
      "Dutch": "オランダ語",
      "Swedish": "スウェーデン語",
      "Norwegian": "ノルウェー語",
      "Danish": "デンマーク語",
      "Finnish": "フィンランド語",
      "Greek": "ギリシャ語",
      "Czech": "チェコ語",
      "Hungarian": "ハンガリー語"
    }
  },
  "errorTexts": {
    "connection_failed": "接続失敗",
    "connection_refused": "接続拒否",
    "server_disconnected": "サーバーとの接続が切断されました",
    "network_communication_error": "ネットワーク通信エラー",
    "send_failed": "データ送信失敗",
    "rate_limit": "操作頻度が高すぎます",
    "reconnecting_countdown": "{1}秒後に再接続（第{0}回）...",
    "reconnecting_attempt": "再接続中（第{0}回）...",
    "reconnect_success": "再接続成功",
    "reconnect_cancel": "再接続キャンセル"
  }
}
```

---

### 3.3 Language Code Mapping

**Client Language Enum** → **Server File Name**:

```typescript
const languageFileMap: Record<string, string> = {
    "ChineseSimplified": "zh-CN.json",
    "ChineseTraditional": "zh-TW.json",
    "English": "en.json",
    "Japanese": "ja.json",
    "Korean": "ko.json",
    "French": "fr.json",
    "German": "de.json",
    "Spanish": "es.json",
    "Italian": "it.json",
    "Portuguese": "pt.json",
    "Russian": "ru.json",
    "Arabic": "ar.json",
    "Turkish": "tr.json",
    "Polish": "pl.json",
    "Dutch": "nl.json",
    "Swedish": "sv.json",
    "Norwegian": "no.json",
    "Danish": "da.json",
    "Finnish": "fi.json",
    "Greek": "el.json",
    "Czech": "cs.json",
    "Hungarian": "hu.json"
};
```

---

### 3.4 Loading Data

**TypeScript Implementation**:

```typescript
import * as fs from 'fs';

function languageToFilename(language: string): string {
    return languageFileMap[language] || "en.json";  // Default to English
}

function loadLocalizationFile(language: string): any {
    const filename = languageToFilename(language);
    const path = `resources/localization/${filename}`;
    return JSON.parse(fs.readFileSync(path, 'utf-8'));
}

function getStartSettingsUI(language: string): StartSettingsUI {
    const texts = loadLocalizationFile(language);
    return {
        accounts: texts.startSettings.accounts,
        general: texts.startSettings.general,
        add_account: texts.startSettings.add_account,
        edit: texts.startSettings.edit,
        delete: texts.startSettings.delete,
        edit_account: texts.startSettings.edit_account,
        account_id: texts.startSettings.account_id,
        password: texts.startSettings.password,
        note_optional: texts.startSettings.note_optional,
        delete_confirm: texts.startSettings.delete_confirm,
        language: texts.startSettings.language,
        font_size: texts.startSettings.font_size,
        ui_sound: texts.startSettings.ui_sound,
        font_size_small: texts.startSettings.font_size_small,
        font_size_medium: texts.startSettings.font_size_medium,
        font_size_large: texts.startSettings.font_size_large,
        font_size_extra_large: texts.startSettings.font_size_extra_large,
        confirm: texts.startSettings.confirm,
        cancel: texts.startSettings.cancel,
        language_names: texts.startSettings.languageNames
    };
}

function getErrorTexts(language: string): ErrorTexts {
    const texts = loadLocalizationFile(language);
    return texts.errorTexts;
}
```

---

## 4. Text Extraction Source

All translations can be extracted from the client's existing `Localization_*.json` files:

**Client Files**:
```
Assets/Resources/Localization_ChineseSimplified.json
Assets/Resources/Localization_ChineseTraditional.json
Assets/Resources/Localization_English.json
Assets/Resources/Localization_Japanese.json
... (22 files total)
```

**Keys to Extract**:
- `start_settings_*` (19 keys)
- `lang_*` (22 keys)
- Network error keys: `connection_failed`, `connection_refused`, `server_disconnected`, `network_communication_error`, `send_failed`, `rate_limit`, `reconnecting_countdown`, `reconnecting_attempt`, `reconnect_success`, `reconnect_cancel` (10 keys)

**Total**: 51 keys × 22 languages = 1,122 text entries

---

## 5. Testing Requirements

### 5.1 Gateway Response Test

**Request**:
```json
{
  "language": "Japanese"
}
```

**Expected Response**:
```json
{
  "servers": [...],
  "ui": {...},
  "start_settings_ui": {
    "accounts": "アカウント",
    "general": "一般設定",
    ...
    "language_names": {
      "ChineseSimplified": "簡体字中国語",
      "English": "英語",
      ...
    }
  }
}
```

---

### 5.2 Login Response Test

**Request**:
```json
{
  "account": "test",
  "password": "password",
  "language": "Korean"
}
```

**Expected Response**:
```json
{
  "success": true,
  ...
  "error_texts": {
    "connection_failed": "연결 실패",
    "server_disconnected": "서버 연결이 끊어졌습니다",
    ...
  }
}
```

---

### 5.3 ChangeLanguage Test

**Request**:
```json
{
  "language": "French"
}
```

**Expected Response**:
```json
{
  "success": true,
  "start_settings_ui": {
    "accounts": "Comptes",
    "general": "Paramètres généraux",
    ...
  },
  "error_texts": {
    "connection_failed": "Échec de la connexion",
    ...
  }
}
```

---

## 6. Implementation Checklist

- [ ] Create 22 localization JSON files in `server/resources/localization/`
- [ ] Extract and translate texts from client `Localization_*.json` files
- [ ] Define `StartSettingsUI` protocol in `.proto` file
- [ ] Define `ErrorTexts` protocol in `.proto` file
- [ ] Define `ChangeLanguageRequest/Response` protocols
- [ ] Modify `GatewayResponse` to include `start_settings_ui` field
- [ ] Modify `LoginResponse` to include `error_texts` field
- [ ] Implement `getStartSettingsUI(language)` function
- [ ] Implement `getErrorTexts(language)` function
- [ ] Implement `handleChangeLanguage(request)` handler
- [ ] Write unit tests for localization loading
- [ ] Write integration tests for Gateway/Login/ChangeLanguage

---

## 7. Deployment Notes

**Pre-Deployment**:
1. Ensure all 22 localization files are complete and tested
2. Verify Protocol definitions are synchronized between client and server
3. Test with all 22 languages

**Deployment Order**:
1. Deploy server with new protocols (backward compatible)
2. Deploy client with SDUI architecture (requires new server)

**Rollback Plan**:
- If server fails: revert to previous version
- If client fails: client reverts to SVN r139 (before SDUI migration)

---

## 8. Future Enhancements

After StartSettings SDUI is stable, consider migrating:
- Hot update texts (13 keys)
- Footer texts
- Other in-game UI texts (Home, Option, Story, etc.)

---

## Appendix: Client-Side Changes (Reference)

**Client will**:
- Simplify `Localization_*.json` to only 5 keys: `loading`, `connecting`, `connection_timeout`, `network_error`, `parse_error`
- Refactor `StartSettings.cs` to use SDUI (receive texts via `OnEnter(args[0])`)
- Refactor `Net.cs` to use server-pushed error texts (stored in `Data.Instance.ErrorTexts`)
- Add handlers for `StartSettingsUI`, `ErrorTexts`, `ChangeLanguageResponse`

**Client will NOT**:
- Use `Localization.Instance.Get()` for any texts except the 5 essential keys
- Store UI texts locally (except the 5 essential keys)
