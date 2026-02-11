# UI界面美术设计规范

## 概述

本文档基于实际的视觉资源和设计规范，定义LOA客户端项目的UI美术设计风格和标准。

### 核心设计哲学

> **"UI设计是数学证明，而非艺术创作。"**

本项目采用系统化、数据驱动的界面设计方法，优先考虑一致性、可复现性和数学和谐性，而非主观美学。

### 设计目标

- **一致性**：数学约束确保所有屏幕的视觉连贯性
- **效率**：最小化自定义资源，减少包体积
- **可维护性**：色彩系统支持快速迭代，无需重新制作资源
- **可访问性**：高对比度深色主题减少长时间使用的视觉疲劳
- **独特性**：复古像素艺术美学在竞争激烈的市场中脱颖而出

---

## 视觉风格定义

### 1. 极简主义（Minimalism）

**核心原则**：拒绝装饰性元素，采用纯粹的几何形状。

**代表性资源：**

| 图片预览 | 图片名称 | 视觉特征 |
|---------|---------|---------|
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/RectangleSolid.png" width="80" style="background-color: #ffffff; padding: 5px;"> | **RectangleSolid.png** | 纯黑色圆角矩形（80x40），无渐变、无纹理 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Sprite.png" width="80" style="background-color: #000000; padding: 5px;"> | **Sprite.png** | 纯白色矩形，用于进度条填充 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Ring.png" width="80" style="background-color: #000000; padding: 5px;"> | **Ring.png** | 简洁的圆环形状，透明背景 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Radar.png" width="80" style="background-color: #ffffff; padding: 5px;"> | **Radar.png** | 径向渐变黑色圆形，复杂度最低 |

**设计规则：**
- 无装饰性细节
- 仅使用纯色填充
- 几何形状保持简洁易识别
- 谨慎使用透明度进行图层叠加效果

**应用方法：**
- 使用单一黑色矩形基础资源（RectangleSolid.png）
- 通过颜色叠加实现不同灰度和色调
- 避免为每个颜色变体创建单独的图片资源

### 2. 像素艺术美学（Pixel Art）

**核心原则**：拥抱低分辨率、网格对齐的艺术风格，致敬8-bit/16-bit时代游戏。

**代表性资源：**

<table>
<tr>
<td width="150" align="center" style="background-color: #ffffff; padding: 10px;">
<img src="../../Assets/Game/HotResources/RawAssets/Texture/Increase.png" width="120"><br/>
<strong>Increase.png</strong>
</td>
<td width="150" align="center" style="background-color: #ffffff; padding: 10px;">
<img src="../../Assets/Game/HotResources/RawAssets/Texture/Decrease.png" width="120"><br/>
<strong>Decrease.png</strong>
</td>
<td width="150" align="center" style="background-color: #ffffff; padding: 10px;">
<img src="../../Assets/Game/HotResources/RawAssets/Texture/Edit.png" width="120"><br/>
<strong>Edit.png</strong>
</td>
<td width="150" align="center" style="background-color: #ffffff; padding: 10px;">
<img src="../../Assets/Game/HotResources/RawAssets/Texture/Focus.png" width="120"><br/>
<strong>Focus.png</strong>
</td>
</tr>
</table>

**Increase.png / Decrease.png**
- 加号/减号图标采用像素化设计
- 边缘锐利，具有可见的像素网格
- 纯黑白配色，无抗锯齿
- 双层轮廓增强视觉深度

**Edit.png**
- 铅笔图标，像素艺术风格
- 锯齿状边缘（齿轮装饰）
- 强烈的复古游戏美学

**Focus.png**
- 像素化的焦点指示器/准星图标
- 明显的8-bit游戏风格
- 高对比度黑白，无中间色调

**设计意图：**
- 向经典游戏美学致敬
- 增强科技感和数字感
- 降低制作成本，保持风格一致性
- 便于快速原型设计和迭代

**制作规范：**
- 所有像素艺术图标均为1024x1024以在高分辨率设备上保持清晰边缘
- 保持像素网格锐利，无模糊效果
- 使用Alpha通道实现透明度，无渐变

### 3. 线条艺术技法（Line Art）

**核心原则**：使用一致的描边粗细定义形状，避免实心填充。

<table>
<tr>
<td width="200" align="center" style="background-color: #000000; padding: 20px;">
<img src="../../Assets/Game/HotResources/RawAssets/Texture/Settings.png" width="150"><br/>
<strong style="color: white;">Settings.png</strong>
</td>
<td width="200" align="center" style="background-color: #000000; padding: 20px;">
<img src="../../Assets/Game/HotResources/RawAssets/Texture/RadiativeRing.png" width="150"><br/>
<strong style="color: white;">RadiativeRing.png</strong>
</td>
</tr>
</table>

**Settings.png - 齿轮图标**
- 细线条勾勒齿轮形状
- 多层同心线条营造深度感
- 白色透明背景，黑色线条
- 高对比度，瞬间可识别

**RadiativeRing.png - 辐射效果**
- 辐射状圆环效果
- 渐变和透明度营造光晕/发光效果
- 适合作为装饰性背景元素
- 用于Login和Initialize界面

**特点：**
- 整体线条粗细一致
- 避免实心填充（仅使用描边）
- 适合动画效果（旋转、脉冲）
- 在不同分辨率下缩放良好

### 4. 深色主题系统（Dark Theme）

**核心原则**：深色背景搭配浅色文本，减少视觉疲劳，营造现代美学。

**按钮状态颜色：**

| 状态 | 颜色值 | RGB | 说明 |
|------|--------|-----|------|
| 正常 | `#FFFFFF` | (255, 255, 255) | 纯白色 |
| 高亮 | `#F5F5F5` | (245, 245, 245) | 浅灰色 |
| 按下 | `#C8C8C8` | (200, 200, 200) | 中灰色 |
| 选中 | `#F5F5F5` | (245, 245, 245) | 同高亮色 |
| 禁用 | `#C8C8C8` 50%透明 | (200, 200, 200, 0.5) | 半透明灰 |

**文本颜色层级（全局统一）：**

| 角色 | RGBA | 十六进制 | 使用场景 |
|------|------|---------|---------|
| 主标题 | `rgba(255, 255, 255, 1)` | `#FFFFFF` | 主标题、重要标签 |
| 正文文本 | `rgba(224, 224, 224, 1)` | `#E0E0E0` | 标准可读内容 |
| 次要信息 | `rgba(192, 192, 192, 1)` | `#C0C0C0` | 辅助信息 |
| 提示文本 | `rgba(160, 160, 160, 1)` | `#A0A0A0` | 占位符、工具提示 |
| 禁用状态 | `rgba(128, 128, 128, 0.5)` | `#80808080` | 非交互元素 |

**背景系统：**
- 基础背景使用纯黑或深灰色调
- 面板背景采用深灰色系
- 所有文本使用白色系确保可读性

**优势：**
- 降低屏幕功耗（OLED显示屏）
- 减少长时间使用的视觉疲劳
- 营造专业/科技感氛围
- 改善对交互元素的关注

---

## 图标设计语言

### 像素图标

**风格特征：**
- 网格对齐像素，无抗锯齿
- 纯黑白（或单色）
- 粗体、易识别的形状
- 复古游戏美学

**图标清单：**

| 预览 | 图标 | 文件 | 尺寸 | 用途 | 风格注释 |
|------|------|------|------|------|---------|
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Increase.png" width="60" style="background-color: #ffffff; padding: 3px;"> | 增加 | Increase.png | 1024x1024 | 加号按钮 | 双层轮廓十字 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Decrease.png" width="60" style="background-color: #ffffff; padding: 3px;"> | 减少 | Decrease.png | 1024x1024 | 减号按钮 | 双层轮廓减号 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Edit.png" width="60" style="background-color: #ffffff; padding: 3px;"> | 编辑 | Edit.png | 1024x1024 | 编辑按钮 | 像素化铅笔，锯齿边缘 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Focus.png" width="60" style="background-color: #ffffff; padding: 3px;"> | 焦点 | Focus.png | - | 焦点指示器 | 准星/目标图标，8-bit风格 |

**设计指南：**
- 在所有缩放级别保持像素网格对齐
- 使用双层轮廓增加深度和可见性
- 保持图标复杂度低（小尺寸下可读）
- 避免颜色填充（依赖轮廓）

### 线条艺术图标

**风格特征：**
- 平滑曲线和一致的描边粗细
- 轮廓形式，无实心填充
- 更高分辨率以保留细节
- 适合动画

**图标清单：**

| 预览 | 图标 | 文件 | 尺寸 | 用途 | 风格注释 |
|------|------|------|------|------|---------|
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Settings.png" width="60" style="background-color: #000000; padding: 3px;"> | 设置 | Settings.png | 2048x2048 | 设置按钮 | 多层齿轮，细线条 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/RadiativeRing.png" width="60" style="background-color: #000000; padding: 3px;"> | 辐射环 | RadiativeRing.png | - | 装饰效果 | 渐变光晕，带透明度 |

**设计指南：**
- 保持线条粗细一致（原生分辨率下2-3px）
- 使用透明度实现分层效果
- 避免缩放不佳的过于复杂的细节
- 适合旋转动画

### 功能性图标

**风格特征：**
- 清晰的符号意义
- 高对比度可见性
- 最小视觉复杂度

**图标清单：**

| 预览 | 图标 | 文件 | 用途 | 风格注释 |
|------|------|------|------|---------|
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/True.png" width="60" style="background-color: #000000; padding: 3px;"> | 对勾 | True.png | 确认标记 | 简单对勾，白色透明背景 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/False.png" width="60" style="background-color: #000000; padding: 3px;"> | 错误 | False.png | 账号系统图标 | 符号表示 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Radar.png" width="60" style="background-color: #ffffff; padding: 3px;"> | 雷达 | Radar.png | 雷达可视化 | 径向渐变圆形 |

### 装饰性元素

**风格特征：**
- 支持视觉层级
- 非交互
- 增强空间感知

**元素清单：**

| 预览 | 元素 | 文件 | 用途 | 风格注释 |
|------|------|------|------|---------|
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Ring.png" width="60" style="background-color: #000000; padding: 3px;"> | 圆环 | Ring.png | 点击效果、遮罩动画 | 简单圆环，用于动态点击效果 |
| <img src="../../Assets/Game/HotResources/RawAssets/Texture/Border.png" width="60" style="background-color: #000000; padding: 3px;"> | 边框 | Border.png | 边框装饰 | 用于视觉分隔的框架元素 |

---

## 色彩系统

### 深色主题调色板

```mermaid
graph TD
    DarkTheme[深色主题核心]
    
    DarkTheme --> Backgrounds[背景]
    DarkTheme --> Foregrounds[前景]
    DarkTheme --> Interactive[交互状态]
    
    Backgrounds --> BG1["纯黑色基础"]
    Backgrounds --> BG2["深灰色面板"]
    
    Foregrounds --> Text[文本层级]
    Foregrounds --> Icons[图标颜色]
    
    Text --> T1["主标题: #FFFFFF<br/>Alpha: 1.0"]
    Text --> T2["正文: #E0E0E0<br/>Alpha: 1.0"]
    Text --> T3["次要: #C0C0C0<br/>Alpha: 1.0"]
    Text --> T4["提示: #A0A0A0<br/>Alpha: 1.0"]
    Text --> T5["禁用: #808080<br/>Alpha: 0.5"]
    
    Icons --> IC1["像素图标: 纯黑色"]
    Icons --> IC2["线条图标: 白色描边"]
    
    Interactive --> Normal["正常: #FFFFFF"]
    Interactive --> Hover["悬停: #F5F5F5"]
    Interactive --> Press["按下: #C8C8C8"]
    Interactive --> Disabled["禁用: #C8C8C880"]
```

### 透明度使用原则

| Alpha值 | 使用场景 | 示例 |
|---------|---------|------|
| `1.0` | 完全可见，主要元素 | 主文本、实心背景 |
| `0.7` | 半透明叠加层 | 圆环叠加层 |
| `0.5` | 禁用状态 | 禁用按钮文本、非交互UI |
| `0.0` | 完全透明 | 纯文本按钮背景 |

### 色彩设计原则

1. **文本颜色由层级决定** - 而非主观偏好
2. **背景使用统一色系** - 黑色和深灰色调
3. **颜色叠加实现变化** - 避免创建大量颜色变体资源
4. **维持对比度** - 最低4.5:1符合WCAG AA标准

---

## 布局数学

### 单位高度系统（83px量子化网格）

**起源：** iPhone 6/7/8标准屏幕高度（1334px）÷ 16 = 83.375px ≈ 83px

**核心规则：** 所有垂直尺寸必须是83px的整数倍。

**理论依据：**
- 强制量子化，消除任意像素值
- 通过一致的间距确保视觉和谐
- 简化响应式设计（缩放网格，而非单个元素）
- 减少布局设计中的决策疲劳

**高度计算公式：**
```
元素高度 = 83px × n（n为正整数）

示例：
标题栏高度 = 83px × 1 = 83px
内容区高度 = 83px × 4 = 332px
按钮高度 = 83px × 1 = 83px
全屏高度 = 83px × 16 = 1328px ≈ 1334px
```

**垂直布局示例：**

```
┌─────────────────────┐
│  标题栏 (1单位)      │  83px
├─────────────────────┤
│                     │
│  内容区域            │  
│  (8单位)            │  664px
│                     │
│                     │
├─────────────────────┤
│  按钮 (1单位)        │  83px
└─────────────────────┘
总计: 10单位 = 830px
```

### 黄金比例（φ ≈ 0.618）

**应用领域：**
- 水平面板宽度分配
- 边距比例
- 嵌套布局递归

**比例常量：**
- 主要部分：φ = 0.618（61.8%）
- 次要部分：1-φ = 0.382（38.2%）

**水平布局示例：**

```
┌───────────────────────┬──────────────┐
│                       │              │
│   主内容面板           │   侧边栏      │
│   (φ = 0.618)         │ (1-φ = 0.382)│
│                       │              │
└───────────────────────┴──────────────┘
    屏幕宽度的61.8%         屏幕宽度的38.2%
```

**递归分割原则：**
- 主面板可进一步按黄金比例分割
- 左侧区域：主面板 × 0.618 = 屏幕的38.2%
- 右侧区域：主面板 × 0.382 = 屏幕的23.6%

### 数学化设计决策树

```mermaid
graph TD
    LayoutDecision[布局决策需求]
    
    LayoutDecision --> Vertical{垂直<br/>尺寸?}
    LayoutDecision --> Horizontal{水平<br/>尺寸?}
    
    Vertical --> UnitCalc["计算:<br/>height = n × 83px"]
    UnitCalc --> ValidateUnit{n是<br/>整数?}
    ValidateUnit -->|是| UseUnit[应用尺寸]
    ValidateUnit -->|否| AdjustUnit[四舍五入到最近整数]
    AdjustUnit --> UseUnit
    
    Horizontal --> GoldenCheck{可应用<br/>黄金比例?}
    GoldenCheck -->|是| CalcGolden["计算:<br/>width = total × 0.618"]
    GoldenCheck -->|否| UseHalf["使用50%或<br/>等分分配"]
    CalcGolden --> UseWidth[应用尺寸]
    UseHalf --> UseWidth
```

---

## 动画与特效风格

### 淡入淡出过渡

**标准时长：** 0.1秒（100毫秒）

**理论依据：**
- 足够快以感觉响应迅速
- 足够慢以可感知（不突兀）
- 所有UI交互保持一致

**应用场景：**
- 按钮状态变化（正常 → 高亮 → 按下）
- 面板显示/隐藏过渡
- 文本颜色变化

### 点击反馈系统

**视觉反馈流程：**

1. **即时反馈：** 按钮颜色变为按下状态（0ms）
2. **动态效果：** Ring.png叠加层在点击位置生成
3. **淡出动画：** Ring叠加层在0.3-0.5秒内淡出
4. **状态恢复：** 按钮恢复正常状态（0.1s过渡）

**叠加层效果规范：**
- 使用Ring.png作为叠加纹理
- 白色，70%不透明度
- 不阻挡后续点击交互
- 径向扩张并淡出

### 粒子效果

**ClickEffect（点击特效）：**
- 使用Ring.png作为粒子纹理
- 径向扩张并淡出
- 出现在遮罩层和交互元素中
- 为用户操作创建触觉反馈

**RadiativeRing（辐射环效果）：**
- Login/Initialize中的静态背景装饰
- 缓慢旋转动画（如果有动画）
- 增强空间深度感知
- 非交互，纯美学

---

## 设计资源管理原则

### 资源复用策略

**优先级层次：**

1. **引擎内置基础图形**（最高优先级）
   - 白色方块
   - 对勾图标
   - 滑块背景
   - 圆形旋钮
   - 输入框背景

2. **自定义资源 + 颜色变化**
   - RectangleSolid.png通过颜色叠加
   - 单一资源实现多种颜色效果

3. **独特自定义资源**（最低优先级，谨慎使用）
   - 仅当基础图形无法满足时使用
   - 示例：Settings齿轮图标、像素艺术UI元素

### 资源管理优势

**包体积控制：**
- 当前：21个PNG文件，16个在使用
- 引擎内置资源占用0字节
- 减少资源下载体积

**性能优化：**
- 引擎内置图形经过优化
- 自定义资源使用单一图集
- 通过批处理减少绘制调用

**可维护性提升：**
- 颜色变化无需重新导出资源
- 跨所有实例一致的视觉更新
- 简化美术与开发协作流程

### 资源创建决策

**决策规则：** 仅当引擎内置图形无法实现所需形状时，才创建自定义资源（例如圆角、特定图标、像素艺术）。

**创建新资源前的检查清单：**
1. 引擎内置图形能否满足需求？
2. 现有资源通过颜色变化能否实现？
3. 该资源是否会在多个地方复用？
4. 文件大小是否合理（建议<100KB）？

---

## 设计风格总结

### 核心关键词

1. **极简主义（Minimalism）** - 拒绝不必要的装饰
2. **像素艺术（Pixel Art）** - 拥抱复古游戏美学
3. **数学化（Mathematical）** - 数据驱动、可复现的决策
4. **深色主题（Dark Theme）** - 现代、护眼的界面
5. **系统化（Systematic）** - 通过约束实现一致性

### 风格标签

**复古未来主义极简风格**（Retro-Futuristic Minimalism）

- **复古（Retro）：** 像素艺术图标，8-bit美学
- **未来（Futuristic）：** 深色主题，简洁几何形状
- **极简（Minimalism）：** 无装饰细节，纯粹功能性
- **数学化（Mathematical）：** 精确的布局系统和比例

### 与现代设计趋势的比较

| 方面 | 本项目 | Material Design | iOS Human Interface |
|------|--------|-----------------|---------------------|
| 色彩系统 | 颜色叠加变化 | 预定义调色板 | 动态颜色提取 |
| 布局 | 数学化（83px, φ） | 8dp网格 | 灵活间距 |
| 图标 | 像素艺术 + 线条艺术 | Material Icons（填充/轮廓） | SF Symbols（权重变体） |
| 主题 | 仅深色 | 明暗支持 | 自适应（自动切换） |
| 动画 | 快速（0.1s） | 中速（0.2-0.3s） | 上下文相关 |
| 哲学 | "数学证明" | "Material隐喻" | "清晰、尊重、深度" |

### 与游戏UI设计趋势的关系

**相似之处：**
- 深色主题在现代游戏中常见（尤其是RPG、策略游戏）
- 像素艺术在独立游戏中复兴
- HUD极简主义增强玩家沉浸感

**差异之处：**
- 游戏UI通常使用拟物化元素（皮革、金属纹理）
- 本项目完全避免纹理
- 无diegetic UI元素（世界内屏幕）

---

## 未来设计方向建议

### 优化机会

1. **整合未使用资源**
   - 当前5个未使用的PNG文件：Circle.png、CircleOutline.png、Author.png、ICON.png、wheelgradient.png
   - 需要决策：删除或记录未来使用场景

2. **消除重复资源**
   - 存在重复的资源文件
   - 统一引用原始资源

3. **图标风格统一**
   - 标准化为像素艺术或线条艺术（目前混用）
   - 建议：保留两者，但明确记录使用上下文

### 潜在扩展

1. **色彩主题变体**
   - 当前：仅深色主题
   - 未来：浅色主题选项（反转文本/背景颜色）
   - 保持相同的对比度和层级结构

2. **无障碍增强**
   - 高对比度模式（增加颜色差异）
   - 更大文字尺寸选项（字号乘以1.25x）
   - 色盲友好调色板（避免仅用红绿指示）

3. **动画丰富度**
   - 当前：简单淡入淡出和颜色过渡
   - 未来：按钮弹性缓动、视差背景
   - 注意：保持性能预算

4. **粒子系统扩展**
   - 当前：基础ClickEffect
   - 未来：上下文粒子（成功=绿色火花，错误=红色闪光）
   - 使用现有Ring.png配合颜色着色

### 需避免的风格一致性陷阱

1. **不要不一致地引入渐变**
   - 当前：仅RadiativeRing和Radar使用渐变（仅装饰）
   - 如在其他地方添加渐变，需建立清晰使用规则

2. **不要违反单位高度系统**
   - 避免使用任意像素值（如100px）
   - 始终使用83px的整数倍

3. **不要添加高分辨率照片纹理**
   - 会与像素艺术/极简美学冲突
   - 如需纹理，使用程序化图案（点、线）

4. **不要引入过多自定义资源**
   - 每个新PNG增加包体积
   - 始终检查：引擎内置图形 + 颜色变化能实现吗？

---

## 附录：视觉参考

### 色彩调色板参考

**文本颜色（深色背景上）：**

| 色块 | 颜色代码 | 用途 | RGB值 |
|------|---------|------|-------|
| <span style="display:inline-block;width:50px;height:20px;background-color:#FFFFFF;border:1px solid #000;"></span> | `#FFFFFF` | 主标题 | (255, 255, 255, 1.0) |
| <span style="display:inline-block;width:50px;height:20px;background-color:#E0E0E0;border:1px solid #000;"></span> | `#E0E0E0` | 正文文本 | (224, 224, 224, 1.0) |
| <span style="display:inline-block;width:50px;height:20px;background-color:#C0C0C0;border:1px solid #000;"></span> | `#C0C0C0` | 次要信息 | (192, 192, 192, 1.0) |
| <span style="display:inline-block;width:50px;height:20px;background-color:#A0A0A0;border:1px solid #000;"></span> | `#A0A0A0` | 提示文本 | (160, 160, 160, 1.0) |
| <span style="display:inline-block;width:50px;height:20px;background-color:#808080;border:1px solid #000;"></span> | `#808080` | 禁用 | (128, 128, 128, 0.5) |

**交互状态：**

| 色块 | 颜色代码 | 状态 | RGB值 |
|------|---------|------|-------|
| <span style="display:inline-block;width:50px;height:20px;background-color:#FFFFFF;border:1px solid #000;"></span> | `#FFFFFF` | 正常 | (255, 255, 255, 1.0) |
| <span style="display:inline-block;width:50px;height:20px;background-color:#F5F5F5;border:1px solid #000;"></span> | `#F5F5F5` | 高亮 | (245, 245, 245, 1.0) |
| <span style="display:inline-block;width:50px;height:20px;background-color:#C8C8C8;border:1px solid #000;"></span> | `#C8C8C8` | 按下 | (200, 200, 200, 1.0) |
| <span style="display:inline-block;width:50px;height:20px;background-color:rgba(200,200,200,0.5);border:1px solid #000;"></span> | `#C8C8C880` | 禁用 | (200, 200, 200, 0.5) |

### 布局网格可视化

```
单位高度系统（83px网格）：

0px   ┬─────────────────────────┐
      │      标题栏              │
83px  ├─────────────────────────┤
      │                         │
      │                         │
      │    内容区域              │
      │     (8单位)             │
      │                         │
      │                         │
      │                         │
747px ├─────────────────────────┤
      │    按钮行                │
830px ┴─────────────────────────┘
```

### 黄金比例布局示例

```
水平分割：
┌───────────────────────┬──────────────┐
│                       │              │
│   主内容              │   侧边栏      │
│   (φ = 0.618)         │ (1-φ = 0.382)│
│                       │              │
└───────────────────────┴──────────────┘
    宽度的61.8%             宽度的38.2%
```

---

## 相关文档

- [UI图片资源使用规范](../System/UI图片资源使用规范.md) - 详细的资源清单和使用跟踪

---

**文档版本：** 1.0  
**最后更新：** 2026-02-11  
**文档类型：** 美术设计规范
