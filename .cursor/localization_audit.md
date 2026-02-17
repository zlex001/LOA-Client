# 客户端本地化文本梳理

## 核心判断标准

**需要客户端本地化的文本定义**：
> 在**网络完全不通**（无法连接任何服务器）时，仍需要显示的文本

---

## 分类汇总

### ✅ 技术必需的客户端本地化（5个）

**定义**：显示时网络尚未连通，无法从服务器获取

| 键名 | 中文文本 | 使用位置 | 必需理由 |
|------|---------|---------|---------|
| `loading` | 载入中... | Gate.cs:61 | 启动时还未连接网关 |
| `connecting` | 连接中... | Start.cs:289 | 点击登录还未建立Socket连接 |
| `connection_timeout` | 连接超时 | Gate.cs:70 | 网关连接超时，未获取服务器数据 |
| `network_error` | 网络错误 | Gate.cs:70 | 网关请求失败，未获取服务器数据 |
| `parse_error` | 数据解析错误 | Gate.cs:81, 112 | 服务器数据异常，无法解析 |

**状态**：✅ 所有22个语言文件都已包含这5个键的翻译

---

### 📋 实现选择的客户端本地化（79个）

**定义**：显示时理论上可以从服务器获取，但基于设计选择使用了客户端本地化

#### 1. 热更新阶段文本（13个）

**特点**：这些文本显示时，已经连上了网关服务器（否则无法下载资源）

| 键名 | 中文文本 | 现状 |
|------|---------|------|
| `connecting_auth` | 正在连接认证服务器... | 未使用 |
| `auth_failed` | 设备认证失败：{0} | 未使用 |
| `checking_version` | 正在检查版本... | 未使用 |
| `version_check_failed` | 版本检查失败：{0} | 未使用 |
| `comparing_resources` | 正在比对资源... | 未使用 |
| `resources_updated` | 资源已是最新 | 未使用 |
| `start_downloading` | 开始下载资源... | 未使用 |
| `downloading_resources` | 下载资源 {0}/{1} | 未使用 |
| `download_complete` | 下载完成 | 未使用 |
| `download_failed` | 下载失败 | 未使用 |
| `unpacking_resources` | 解压资源中 {0}% | 未使用 |
| `loading_metadata` | 正在加载元数据... | 未使用 |
| `launching_game` | 正在启动游戏... | 未使用 |
| `resource_check_failed` | 资源检查失败，请检查网络连接后重试 | 未使用 |
| `metadata_load_failed` | 元数据加载失败 | 未使用 |
| `launch_failed` | 启动游戏失败：{0} | 未使用 |

**理论上**：这些可以由网关服务器在首次响应时推送

#### 2. 游戏中网络错误（11个）

**特点**：显示时用户已登录游戏，服务器可以在登录/初始化时推送这些文本

| 键名 | 中文文本 | 使用位置 |
|------|---------|---------|
| `connection_failed` | 连接失败 | Net.cs:182 |
| `connection_refused` | 服务器拒绝连接 | Net.cs:173 |
| `server_disconnected` | 与服务器的连接已断开 | Net.cs:234, 505 |
| `network_communication_error` | 网络通信异常 | Net.cs:266, 272 |
| `send_failed` | 发送数据失败 | Net.cs:315, 321 |
| `rate_limit` | 操作频率过快 | Net.cs:604 |
| `reconnecting_countdown` | {1}秒后重连（第{0}次）... | Net.cs:670 |
| `reconnecting_attempt` | 正在重连（第{0}次）... | Net.cs:679 |
| `reconnect_success` | 重连成功 | Net.cs:689 |
| `reconnect_cancel` | 已取消重连 | (未使用) |

**注意**：`connection_timeout`在Net.cs:146也有使用，但此时理论上可以用之前服务器推送的文本

**理论上**：这些可以由游戏服务器在登录/初始化响应时推送

---

## 详细清单（原84个键的完整列表）

### 1. 热更新/启动阶段（18个）
**必需性**：✅ 关键 - 客户端启动、连接服务器、热更新时显示

| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `loading` | 载入中... | Gate.cs:61 | 启动时连接网关 |
| `connecting` | 连接中... | Start.cs:289, UI.cs:559 | 登录连接提示 |
| `connecting_auth` | 正在连接认证服务器... | (未使用) | 认证服务器连接 |
| `auth_failed` | 设备认证失败：{0} | (未使用) | 设备认证失败 |
| `checking_version` | 正在检查版本... | (未使用) | 版本检查 |
| `version_check_failed` | 版本检查失败：{0} | (未使用) | 版本检查失败 |
| `comparing_resources` | 正在比对资源... | (未使用) | 资源比对 |
| `resources_updated` | 资源已是最新 | (未使用) | 资源已最新 |
| `start_downloading` | 开始下载资源... | (未使用) | 开始下载 |
| `downloading_resources` | 下载资源 {0}/{1} | (未使用) | 下载进度 |
| `download_complete` | 下载完成 | (未使用) | 下载完成 |
| `download_failed` | 下载失败 | (未使用) | 下载失败 |
| `unpacking_resources` | 解压资源中 {0}% | (未使用) | 解压进度 |
| `loading_metadata` | 正在加载元数据... | (未使用) | 加载元数据 |
| `launching_game` | 正在启动游戏... | (未使用) | 启动游戏 |
| `resource_check_failed` | 资源检查失败，请检查网络连接后重试 | (未使用) | 资源检查失败 |
| `metadata_load_failed` | 元数据加载失败 | (未使用) | 元数据加载失败 |
| `launch_failed` | 启动游戏失败：{0} | (未使用) | 游戏启动失败 |

### 2. 网络错误提示（11个）
**必需性**：✅ 关键 - 网络异常时必须显示

| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `network_error` | 网络错误 | Gate.cs:70 | 网关请求失败 |
| `parse_error` | 数据解析错误 | Gate.cs:81, 112 | 数据解析失败 |
| `connection_timeout` | 连接超时 | Gate.cs:70, Net.cs:146 | 连接超时 |
| `connection_refused` | 服务器拒绝连接 | Net.cs:173 | 服务器拒绝 |
| `connection_failed` | 连接失败 | Net.cs:182 | 连接失败 |
| `server_disconnected` | 与服务器的连接已断开 | Net.cs:234, 505 | 服务器断开 |
| `network_communication_error` | 网络通信异常 | Net.cs:266, 272 | 通信异常 |
| `send_failed` | 发送数据失败 | Net.cs:315, 321 | 发送失败 |
| `rate_limit` | 操作频率过快 | Net.cs:604 | 频率限制 |
| `reconnect_success` | 重连成功 | Net.cs:689 | 重连成功提示 |
| `reconnect_cancel` | 已取消重连 | (未使用) | 取消重连提示 |

### 3. 重连提示（2个）
**必需性**：✅ 必需 - 重连过程提示

| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `reconnecting_countdown` | {1}秒后重连（第{0}次）... | Net.cs:670 | 重连倒计时 |
| `reconnecting_attempt` | 正在重连（第{0}次）... | Net.cs:679 | 重连尝试中 |

#### 3. StartSettings界面（30个）

**特点**：能打开StartSettings说明之前已连上服务器，理论上可以由服务器提前推送

##### 3.1 区域标题（4个）
| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `start_settings_accounts` | 账号 | StartSettings.cs:323 | 账号区域标题 |
| `start_settings_general` | 通用设置 | StartSettings.cs:343 | 通用设置区域标题 |
| `start_settings_tab_account` | 账号 | (未使用) | 账号标签页 |
| `start_settings_tab_settings` | 设置 | (未使用) | 设置标签页 |

##### 3.2 账号管理（13个）
| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `start_settings_add_account` | 添加账号 | StartSettings.cs:514, 1137 | 添加账号按钮/标题 |
| `start_settings_edit` | 编辑 | StartSettings.cs:442 | 编辑按钮 |
| `start_settings_delete` | 删除 | StartSettings.cs:475 | 删除按钮 |
| `start_settings_edit_account` | 编辑账号 | StartSettings.cs:1221 | 编辑账号对话框标题 |
| `start_settings_account_id` | 账号ID | StartSettings.cs:1147, 1231 | 账号ID输入框 |
| `start_settings_password` | 密码 | StartSettings.cs:1152, 1236 | 密码输入框 |
| `start_settings_note_optional` | 备注（可选） | StartSettings.cs:1158, 1242 | 备注输入框 |
| `start_settings_edit_note` | 编辑备注 | (未使用) | 编辑备注 |
| `start_settings_delete_confirm` | 确定要删除账号 {0} 吗？ | StartSettings.cs:939 | 删除确认简化消息 |
| `start_settings_delete_confirm_title` | 删除账号 | (未使用) | 删除确认对话框标题 |
| `start_settings_delete_confirm_message` | 账号：{0}\n备注：{1}\n\n此操作无法撤销。 | (未使用) | 删除确认详细消息 |
| `start_settings_cannot_delete_current` | 无法删除当前使用的账号 | (未使用) | 无法删除提示 |
| `start_settings_id_password_required` | 账号ID和密码为必填项 | (未使用) | 必填项提示 |

##### 3.3 设置项（9个）
| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `start_settings_language` | 语言 | StartSettings.cs:554 | 语言设置标签 |
| `start_settings_font_size` | 字体大小 | StartSettings.cs:622 | 字体设置标签 |
| `start_settings_ui_sound` | 音效 | StartSettings.cs:836 | 音效设置标签 |
| `start_settings_sound_on` | 开启 | (未使用) | 音效开启 |
| `start_settings_sound_off` | 关闭 | (未使用) | 音效关闭 |
| `font_size_small` | 小 | StartSettings.cs:726 | 字体：小 |
| `font_size_medium` | 中 | StartSettings.cs:727 | 字体：中 |
| `font_size_large` | 大 | StartSettings.cs:728 | 字体：大 |
| `font_size_extra_large` | 特大 | StartSettings.cs:729 | 字体：特大 |

##### 3.4 对话框按钮（4个）
| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `start_settings_confirm` | 确认 | StartSettings.cs:1167, 1251, 1381, 1506 | 确认按钮 |
| `start_settings_cancel` | 取消 | StartSettings.cs:1162, 1246, 1376, 1501 | 取消按钮 |
| `start_settings_error` | 错误 | (未使用) | 错误标题 |

#### 4. 语言名称（22个）

**特点**：StartSettings使用，理论上可以由服务器推送

| 键名 | 中文示例 | 说明 |
|------|---------|------|
| `lang_ChineseSimplified` | 简体中文 | 简体中文 |
| `lang_ChineseTraditional` | 繁體中文 | 繁体中文 |
| `lang_English` | English | 英语 |
| `lang_Japanese` | 日本語 | 日语 |
| `lang_Korean` | 한국어 | 韩语 |
| `lang_French` | Français | 法语 |
| `lang_German` | Deutsch | 德语 |
| `lang_Spanish` | Español | 西班牙语 |
| `lang_Portuguese` | Português | 葡萄牙语 |
| `lang_Russian` | Русский | 俄语 |
| `lang_Italian` | Italiano | 意大利语 |
| `lang_Polish` | Polski | 波兰语 |
| `lang_Turkish` | Türkçe | 土耳其语 |
| `lang_Dutch` | Nederlands | 荷兰语 |
| `lang_Danish` | Dansk | 丹麦语 |
| `lang_Swedish` | Svenska | 瑞典语 |
| `lang_Norwegian` | Norsk | 挪威语 |
| `lang_Finnish` | Suomi | 芬兰语 |
| `lang_Thai` | ไทย | 泰语 |
| `lang_Vietnamese` | Tiếng Việt | 越南语 |
| `lang_Indonesian` | Bahasa Indonesia | 印尼语 |
| `lang_Ukrainian` | Українська | 乌克兰语 |

#### 5. 其他（1个）

| 键名 | 中文文本 | 使用位置 | 说明 |
|------|---------|---------|------|
| `footer` | 客户端版本{0} 设备号{1} | (Start SDUI) | 版本信息（已由服务器推送）

---

## 统计汇总

| 分类 | 键数量 | 技术必需性 | 翻译状态 |
|------|--------|-----------|---------|
| **技术必需的客户端本地化** | **5** | ✅ 绝对必需 | ✅ 已完成 |
| 热更新/启动阶段（预留） | 13 | ❌ 实现选择 | ✅ 已完成 |
| 游戏中网络错误 | 11 | ❌ 实现选择 | ✅ 已完成 |
| StartSettings界面 | 30 | ❌ 实现选择 | ❌ 需补全 |
| 语言名称 | 22 | ❌ 实现选择 | ✅ 已完成 |
| 其他 | 1 | ❌ 实现选择 | ✅ 已完成 |
| **总计** | **84** | - | - |

---

## 核心结论

### ✅ 技术必需的客户端本地化（5个）

**按照"网络完全不通时仍需显示"的严格标准**：

1. `loading` - 载入中...（尝试连接网关时）
2. `connecting` - 连接中...（点击登录尝试建立Socket时）
3. `connection_timeout` - 连接超时（网关连接超时）
4. `network_error` - 网络错误（网关请求失败）
5. `parse_error` - 数据解析错误（网关数据异常）

**这5个文本是唯一在完全无法连接服务器时必须显示的，因此必须使用客户端本地化。**

**状态**：✅ 所有22个语言文件都已完整翻译

---

### 📋 实现选择的客户端本地化（79个）

**这些文本理论上可以由服务器推送（采用SDUI架构），但当前实现选择了客户端本地化**：

#### 原因分析

1. **游戏中网络错误**（11个）
   - 用户已登录游戏，服务器可以在登录响应中推送这些错误文本
   - 当前选择客户端本地化，可能是为了简化服务器逻辑

2. **热更新文本**（13个预留，未使用）
   - 热更新时已连上网关，可以由网关推送文本
   - 当前预留在客户端，可能为未来热更新功能准备

3. **StartSettings界面**（30个）
   - 用户能打开此界面说明之前已连上服务器
   - 当前选择客户端本地化，原因可能是：
     - 架构清晰（纯客户端界面独立于服务器）
     - 开发便利（不需要服务器推送配置界面文本）
     - 设计选择（设置界面传统上由客户端控制）

4. **语言名称**（22个）
   - StartSettings使用，理论上可以由服务器推送
   - 选择客户端本地化，因为语言名称是静态数据

---

## 当前问题

### ❌ StartSettings键翻译缺失

**问题**：30个StartSettings键在20个非中英文语言中仍是英文占位符

**影响**：切换到这些语言时，StartSettings界面显示英文而非对应语言

**解决方案**：补全20个语言的翻译

---

## 架构思考

### 是否应该将StartSettings改为SDUI？

**优点**：
- 架构统一（所有登录后界面都用SDUI）
- 翻译统一管理（服务器端）
- 无需客户端打包包含所有语言翻译

**缺点**：
- 设置界面依赖服务器（传统上设置是纯客户端）
- 增加服务器复杂度
- 用户体验可能不如客户端本地化即时

**当前选择**：保持客户端本地化，补全翻译

---

## 下一步行动

### 必须执行
- 为20个非中英文语言补全30个StartSettings键的翻译

### 架构决策（需讨论）
- 是否将StartSettings改为SDUI架构？
- 是否将游戏中网络错误改为SDUI推送？
