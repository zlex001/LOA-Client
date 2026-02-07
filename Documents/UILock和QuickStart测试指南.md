# UILock和QuickStart功能测试指南

## 功能概述

本次更新实现了两个核心功能：
1. **UILock协议**：支持Home界面子面板的渐进式显示
2. **QuickStart功能**：游客快速登录，跳过传统注册流程

## 前置准备

### 1. Unity预制体修改（必需）

在开始测试前，需要在Unity编辑器中手动添加QuickStart按钮：

1. 打开 `Assets/Resources/Prefabs/UI/Start.prefab`
2. 复制 `Login` 按钮，重命名为 `QuickStart`
3. 修改QuickStart按钮的子对象Text组件：
   - Text内容将由服务器通过协议推送
   - 或使用本地化Key："quickstart"
4. 调整按钮位置（代码已自动处理布局）
5. 保存预制体

### 2. 本地化文本（可选）

如果服务器不通过Start协议推送QuickStart按钮文本，需要在本地化文件中添加：

**中文** (`Resources/Localization_ChineseSimplified.json`)：
```json
{
  "quickstart": "快速开始",
  "quickstart_tooltip": "创建账号并立即开始游戏"
}
```

**英文** (`Resources/Localization_English.json`)：
```json
{
  "quickstart": "Quick Start",
  "quickstart_tooltip": "Create account and start playing immediately"
}
```

## 测试场景

### 场景1：QuickStart快速登录

**前置条件**：
- 服务器已实现QuickStart协议处理
- 服务器能够返回LoginResponse（Code.Success）

**测试步骤**：
1. 启动客户端
2. 在Start界面点击"快速开始"按钮
3. 观察控制台日志

**预期结果**：
- 显示"连接中..."加载界面
- 控制台输出：
  ```
  [Start] QuickStart button clicked
  [Start] Selected server: {服务器名称}
  [Net] LoginAccount changed. Online: False
  [Net] Not online, initiating TCP connection to {IP}:{Port}
  [Net] Connected, sending QuickStartRequest
  ```
- 成功登录后进入Home界面

**验证点**：
- QuickStartRequest协议字段正确：
  - `device`: 设备唯一标识
  - `version`: 客户端版本号
  - `platform`: 平台类型（WindowsPlayer/OSXPlayer/Android等）
  - `language`: 语言代码（ChineseSimplified/English等）

### 场景2：UILock初始状态（只显示Scene）

**前置条件**：
- 服务器在登录后推送UILock协议：
  ```json
  {
    "unlockedPanels": ["Home.Scene"]
  }
  ```

**测试步骤**：
1. QuickStart登录成功
2. 进入Home界面

**预期结果**：
- Scene面板全屏显示（占满整个屏幕）
- 其他面板隐藏：
  - Area（角色列表）不显示
  - Information（信息面板）不显示
  - Resource（资源条）不显示
  - Chat（聊天面板）不显示
- 地图可以正常滚动和缩放

**验证点**：
- Scene面板尺寸 = 屏幕尺寸
- 其他面板的GameObject.activeSelf = false

### 场景3：UILock渐进解锁（教程推进）

**前置条件**：
- 服务器在教程推进时推送新的UILock协议

**测试步骤**：
1. 初始状态：只显示Scene
2. 服务器推送新UILock：
   ```json
   {
     "unlockedPanels": ["Home.Scene", "Home.Area", "Home.Information"]
   }
   ```
3. 观察界面变化

**预期结果**：
- Scene面板缩小到标准尺寸（黄金比例布局）
- Area面板出现在右侧
- Information面板出现在上方
- Resource和Chat仍然隐藏

**验证点**：
- 布局切换平滑（从全屏到标准布局）
- 面板位置符合设计规范（黄金比例）
- GridView正确刷新

### 场景4：UILock完全解锁（教程完成）

**前置条件**：
- 服务器推送完整UILock：
  ```json
  {
    "unlockedPanels": [
      "Home.Scene",
      "Home.Area",
      "Home.Information",
      "Home.Resource",
      "Home.Chat"
    ]
  }
  ```

**测试步骤**：
1. 从部分解锁状态推进
2. 服务器推送完整UILock

**预期结果**：
- 所有面板显示
- 标准布局（与传统登录一致）
- 所有功能正常可用

### 场景5：向后兼容（传统登录）

**前置条件**：
- 使用传统账号密码登录
- 服务器不发送UILock协议

**测试步骤**：
1. 在Start界面使用账号登录（不使用QuickStart）
2. 进入Home界面

**预期结果**：
- 所有面板正常显示
- 无任何异常或错误
- 功能与之前版本完全一致

**验证点**：
- Data.Instance.UILock == null
- 所有面板的GameObject.activeSelf = true

### 场景6：屏幕适配

**测试步骤**：
1. 在不同分辨率下测试：
   - 1920x1080
   - 1280x720
   - 2560x1440
2. 测试UILock各个状态

**预期结果**：
- 全屏Scene布局正确适配
- 标准布局正确适配
- 黄金比例计算正确

### 场景7：运行时动态切换

**测试步骤**：
1. Home界面打开状态下
2. 服务器多次推送不同的UILock协议
3. 观察界面动态变化

**预期结果**：
- 每次UILock变化都能正确响应
- 布局平滑切换
- 无UI闪烁或错位

## 调试技巧

### 查看UILock状态

在Unity编辑器的Hierarchy中选择Data对象，在Inspector中观察：
- `raw` 字典中的 `UILock` 条目
- 查看 `unlockedPanels` 列表

### 控制台日志

关键日志输出：
```
[Start] QuickStart button clicked
[Net] QuickStart mode detected, sending QuickStartRequest
[Net] Connected, sending QuickStartRequest
[Home] Applying UILock: {panel count}
```

### 手动触发UILock（测试用）

在Unity编辑器运行时，可以在Console中执行：
```csharp
// 测试只显示Scene
var uiLock = new Game.Protocol.UILock { 
    unlockedPanels = new List<string> { "Home.Scene" } 
};
Game.Data.Instance.UILock = uiLock;

// 测试部分解锁
var uiLock2 = new Game.Protocol.UILock { 
    unlockedPanels = new List<string> { 
        "Home.Scene", "Home.Area", "Home.Information" 
    } 
};
Game.Data.Instance.UILock = uiLock2;

// 测试完全解锁
Game.Data.Instance.UILock = null;
```

## 常见问题排查

### 问题1：QuickStart按钮不存在

**症状**：点击按钮无反应，控制台无日志

**原因**：未在Unity预制体中添加QuickStart按钮

**解决**：按照"前置准备"章节添加QuickStart按钮

### 问题2：UILock不生效

**症状**：收到UILock协议但面板不变化

**检查点**：
1. 确认协议格式正确
2. 确认面板名称匹配：`Home.Scene`（不是`Scene`）
3. 检查Home界面是否已注册UILock监听
4. 查看控制台是否有错误

### 问题3：布局错乱

**症状**：面板位置不正确或重叠

**检查点**：
1. 确认屏幕适配值正确
2. 检查RectTransform设置
3. 尝试切换分辨率测试

### 问题4：QuickStart重复创建账号

**说明**：这是服务器端问题，客户端每次都会发送device字段

**服务器需要**：
- 根据device字段判断是否已创建账号
- 如已存在则直接登录，不重复创建

## 测试检查清单

- [ ] QuickStart按钮已在Unity中添加
- [ ] QuickStart登录流程正常
- [ ] 初始只显示Scene面板（全屏）
- [ ] Scene全屏布局正确
- [ ] UILock渐进解锁正常
- [ ] 布局动态切换平滑
- [ ] 完全解锁后所有面板显示
- [ ] 传统登录不受影响
- [ ] 不同分辨率适配正确
- [ ] 运行时动态切换正常
- [ ] 无控制台错误或警告

## 服务器端配合

### 需要服务器实现的协议

1. **QuickStartRequest处理**：
   - 接收字段：device, version, platform, language
   - 返回：LoginResponse（复用现有协议）
   - 逻辑：根据device创建或登录账号

2. **UILock推送**：
   - 登录后推送初始UILock（如只有Scene）
   - 教程推进时推送新UILock
   - 老玩家不推送UILock（或推送完整列表）

### 测试协议示例

使用Postman或类似工具模拟服务器推送：

**QuickStartRequest**：
```json
{
  "device": "TEST-DEVICE-001",
  "version": "1.0.0",
  "platform": "WindowsPlayer",
  "language": "ChineseSimplified"
}
```

**UILock（初始）**：
```json
{
  "unlockedPanels": ["Home.Scene"]
}
```

**UILock（进度1）**：
```json
{
  "unlockedPanels": [
    "Home.Scene",
    "Home.Area",
    "Home.Information"
  ]
}
```

**UILock（完成）**：
```json
{
  "unlockedPanels": [
    "Home.Scene",
    "Home.Area",
    "Home.Information",
    "Home.Resource",
    "Home.Chat"
  ]
}
```

## 性能注意事项

- UILock变化会触发布局重新计算
- 建议在教程关键节点推送，避免频繁推送
- 布局切换时会刷新GridView，可能有轻微卡顿（正常现象）

## 后续优化建议

1. **动画效果**：可添加DOTween淡入/滑入动画
2. **过渡效果**：布局切换时添加平滑过渡
3. **锁定提示**：未解锁面板显示"即将解锁"提示
4. **教程引导**：结合Tutorial系统高亮新解锁面板

---

测试完成后请反馈问题至开发团队。
