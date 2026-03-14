# Settings panel text keys audit (StartSettings)

Client reads all UI strings from `DataManager.Instance.Texts` via `GetText(key)`. This file lists every key used and where it must come from.

## 1. Keys used by client (exact strings)

| Key | Used in | Server 语言.csv cid | Server→client mapping |
|-----|---------|---------------------|------------------------|
| general | CreateSectionHeader | StartSettings.general | same |
| language | CreateLanguageItem label | StartSettings.language | same |
| font_size | CreateFontSizeItem label | StartSettings.fontSize | fontSize → font_size |
| font_size_small | Font size value | StartSettings.fontSizeSmall | fontSizeSmall → font_size_small |
| font_size_medium | Font size value | StartSettings.fontSizeMedium | fontSizeMedium → font_size_medium |
| font_size_large | Font size value | StartSettings.fontSizeLarge | fontSizeLarge → font_size_large |
| font_size_extra_large | Font size value | StartSettings.fontSizeExtraLarge | fontSizeExtraLarge → font_size_extra_large |
| ui_sound | CreateSoundItem label | StartSettings.uiSound | uiSound → ui_sound |
| accounts | CreateSectionHeader | StartSettings.accounts | same |
| edit | Account edit button | StartSettings.edit | same |
| delete | Account delete button | StartSettings.delete | same |
| add_account | Add account button | StartSettings.addAccount | addAccount → add_account |
| delete_confirm | Delete confirm dialog | StartSettings.deleteConfirm | deleteConfirm → delete_confirm |
| edit_account | Dialog title | StartSettings.editAccount | editAccount → edit_account |
| account_id | Dialog field label | StartSettings.accountId | accountId → account_id |
| password | Dialog field label | StartSettings.password | same |
| note_optional | Dialog field label | StartSettings.noteOptional | noteOptional → note_optional |
| cancel | Dialog button | StartSettings.cancel | same |
| confirm | Dialog button | StartSettings.confirm | same |
| lang_{Enum} | Language value (e.g. lang_English) | StartSettings.lang{Enum} (e.g. langEnglish) | langXxx → lang_Xxx |

## 2. Client Languages enum (22) vs 语言.csv

Each must have a row `StartSettings.lang{Enum}` in 语言.csv (e.g. langIndonesian). After strip prefix the server key is `langIndonesian`; `ToClientSettingsKey` adds `lang_Indonesian` to the Gateway response.

- ChineseSimplified, ChineseTraditional, English, Japanese, Korean, French, German, Spanish, Portuguese, Russian, Italian, Polish, Turkish, Dutch, Danish, Swedish, Norwegian, Finnish, Thai, Vietnamese, Indonesian, Ukrainian.

## 3. Data flow

- **Source**: `Server/Library/Design/语言.csv` (design table).
- **Generate**: Run with `IsDevelopment` (or your Convert step) → `Server/Library/Config/Multilingual.csv`.
- **Runtime**: `Data.Texts.Init()` loads `Paths.Config/Multilingual.csv` → CidToId + Multilingual.
- **API**: `Logic.Text.Agent.GetGatewayTexts(lang)` builds dict; for each StartSettings.* key it adds both server key and `ToClientSettingsKey(key)` when present.
- **Client**: Gateway response sets `DataManager.Instance.Texts`; StartSettings uses `GetText(key)`. If key missing or empty, `GetText` returns the key so the UI never shows blank.

## 4. Client defensive behavior (after this audit)

- `GetText(key)`: if value is null or empty, returns `key` (no blank label).
- Font size value: each of the four font label keys is fallbacked to the key if empty.
- Language name: `GetLanguageName` returns key if result is empty.
