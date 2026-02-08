# Unity项目结构规范

定义LOA客户端Unity项目的目录组织、程序集划分、命名空间约定等规范。

## 程序集划分

### 三层程序集结构

```
Assembly-CSharp（Unity主程序集）
    ↓
Framework程序集（基础设施）
    ↓
Game程序集（热更新代码）
```

### 1. Assembly-CSharp（Unity主程序集）

**位置**：`Assets/Basic/`

**职责**：
- Unity启动入口（Main.cs）
- 热更新检查和下载
- 程序集动态加载

**特点**：
- 非热更新代码
- 编译到APK/IPA中
- 无法运行时修改

**包含文件**：
- `Main.cs`：Unity启动入口
- `Config.asset`：配置资源文件

### 2. Framework 程序集

**位置**：`Assets/Framework/`

**程序集定义**：`Framework.asmdef`

**依赖**：无（仅依赖Unity引擎）

**职责**：
- Unity相关基础设施
- Singleton基类
- AssetManager资源管理
- Http网络请求
- Hot热更新管理
- Localization本地化

**特点**：
- 非热更新代码
- 可被Game程序集引用
- 提供稳定的基础能力

### 3. Game 程序集

**位置**：`Assets/Game/Scripts/`

**程序集定义**：`Game.asmdef`

**依赖**：Framework程序集

**职责**：
- 游戏逻辑代码
- 管理器层（Data、Net、UI、Audio）
- 界面实现
- 协议定义

**特点**：
- 可热更新代码
- 编译为game.dll.bytes
- 运行时动态加载

---

## 目录结构

### Assets/ 目录组织

```
Assets/
├── Basic/                      # Unity主程序集
│   ├── Main.cs                 # Unity启动入口
│   └── Config.asset            # 配置资源
│
├── Framework/                  # Framework程序集
│   ├── Framework.asmdef        # 程序集定义
│   ├── Singleton.cs            # 单例基类
│   ├── AssetManager.cs         # 资源管理
│   ├── Http.cs                 # HTTP请求
│   ├── Hot.cs                  # 热更新
│   ├── Localization.cs         # 本地化
│   └── ...
│
├── Game/                       # Game程序集
│   ├── Scripts/
│   │   ├── Game.asmdef         # 程序集定义
│   │   ├── Basic/              # 基础设施
│   │   │   ├── Gate.cs         # 热更新入口
│   │   │   ├── Flow.cs         # 流程状态机
│   │   │   ├── Monitor.cs      # 事件系统
│   │   │   └── Event.cs        # 全局事件
│   │   ├── Data/               # 数据管理
│   │   │   ├── Data.cs         # 数据管理器
│   │   │   ├── Config.cs       # 配置管理
│   │   │   └── Local.cs        # 本地存储
│   │   ├── Network/            # 网络通信
│   │   │   ├── Net.cs          # 网络管理器
│   │   │   └── Protocol.cs     # 协议定义
│   │   ├── UI/                 # 表现层
│   │   │   ├── Core/           # UI核心
│   │   │   ├── Common/         # 通用组件
│   │   │   ├── Start/          # 启动界面
│   │   │   ├── Home/           # 主界面
│   │   │   └── ...
│   │   ├── Utils/              # 工具类
│   │   ├── ThirdParty/         # 第三方库
│   │   └── SDK/                # 平台SDK
│   ├── HotResources/           # 热更新资源
│   └── Res/                    # 资源文件
│
├── Resources/                  # Unity Resources
│   └── Localization/           # 多语言文件
│
├── StreamingAssets/            # 流式资源
│   └── game.dll.bytes          # 热更新DLL
│
├── Plugins/                    # 第三方插件
│
└── Editor/                     # 编辑器工具
```

---

## 命名空间约定

### 规则：命名空间反映目录结构

```csharp
// 目录 → 命名空间
Assets/Game/Scripts/                 → namespace Game
Assets/Game/Scripts/Basic/           → namespace Game或Game.Basic
Assets/Game/Scripts/Data/            → namespace Game
Assets/Game/Scripts/Network/         → namespace Game.Network
Assets/Game/Scripts/UI/Home/         → namespace Game.UI.Home
Assets/Framework/                    → namespace Framework
```

### 示例

```csharp
// Data.cs 位于 Assets/Game/Scripts/Data/
namespace Game
{
    public class Data : Singleton<Data> { }
}

// Net.cs 位于 Assets/Game/Scripts/Network/
namespace Game
{
    public class Net : Singleton<Net> { }
}

// Home.cs 位于 Assets/Game/Scripts/UI/Home/
namespace Game.UI.Home
{
    public class Home : UIBase { }
}

// Hot.cs 位于 Assets/Framework/
namespace Framework
{
    public class Hot : Singleton<Hot> { }
}
```

### 例外

- **ThirdParty/**：保持第三方库原有命名空间
- **SDK/**：使用平台相关命名空间（如 `namespace Game.SDK.Android`）

---

## 文件归属标准

### 管理器类

| 类型 | 目录 | 命名空间 |
|------|------|----------|
| Data管理器 | `Data/Data.cs` | `Game` |
| Net管理器 | `Network/Net.cs` | `Game` |
| UI管理器 | `UI/Core/UI.cs` | `Game` |
| Audio管理器 | `Data/Audio.cs` | `Game` |

### UI界面类

| 类型 | 目录 | 命名空间 |
|------|------|----------|
| 启动界面 | `UI/Start/Start.cs` | `Game.UI.Start` |
| 主界面 | `UI/Home/Home.cs` | `Game.UI.Home` |
| 剧情界面 | `UI/Story/Story.cs` | `Game.UI.Story` |

### 工具类

| 类型 | 目录 | 命名空间 |
|------|------|----------|
| 扩展方法 | `Utils/Extensions/` | `Game.Utils` |
| 辅助类 | `Utils/Helpers/` | `Game.Utils` |
| 组件 | `Utils/Components/` | `Game.Utils` |

---

## 不可移动文件

以下文件由系统自动生成或管理，**不得手动移动或删除**：

### Game 程序集

- `Game.asmdef`：程序集定义文件
- `game.dll.bytes`：热更新DLL（构建产物）
- `HybridCLRGenerate/`：HybridCLR生成代码目录

### Framework 程序集

- `Framework.asmdef`：程序集定义文件

### Unity系统文件

- `ProjectSettings/`：Unity项目设置
- `Packages/`：Unity包管理
- `Library/`：Unity缓存（.gitignore）
- `Temp/`：临时文件（.gitignore）

---

## 资源目录

### Resources/ 目录

**用途**：Unity Resources API 加载的资源

**内容**：
- `Localization/`：多语言JSON文件
- 其他需要 `Resources.Load()` 加载的资源

**注意**：
- Resources 资源会打包到主包，增加包体积
- 优先使用 AssetBundle 或 Addressable

### StreamingAssets/ 目录

**用途**：流式资源，原封不动打包

**内容**：
- `game.dll.bytes`：热更新DLL
- `game.dll.pdb.bytes`：调试信息（可选）
- 元数据DLL（mscorlib.dll.bytes等）

**注意**：
- 首包包含初始版本
- 热更新时下载新版本到 persistentDataPath

### HotResources/ 目录

**用途**：热更新资源（AssetBundle）

**内容**：
- UI预制体
- 场景资源
- 配置文件

**注意**：
- 构建为 AssetBundle
- 支持热更新

---

## 程序集引用规则

### 允许的引用

```
Game.asmdef → Framework.asmdef → Unity引擎
```

- ✅ Game 可引用 Framework
- ✅ Framework 可引用 Unity引擎
- ✅ 所有程序集可引用 ThirdParty（第三方库）

### 禁止的引用

- ❌ Framework 不得引用 Game
- ❌ Assembly-CSharp 不得引用 Game（热更新代码）
- ❌ 循环引用

---

## 构建产物

### 开发构建

**输出目录**：`Build/Development/`

**包含**：
- APK/IPA/EXE
- game.dll.bytes（打包在StreamingAssets中）
- HotResources AssetBundles

### 发布构建

**输出目录**：`Build/Release/`

**包含**：
- APK/IPA/EXE（代码和资源剥离优化）
- game.dll.bytes
- HotResources AssetBundles

### 热更新资源

**输出目录**：`Build/HotUpdate/`

**包含**：
- game.dll.bytes
- game.dll.pdb.bytes（可选）
- 元数据DLL
- AssetBundles
- resdepend.txt（资源依赖）
- res.txt（资源列表）

---

## .gitignore 规范

### 应该忽略的目录/文件

```gitignore
# Unity 生成
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/

# 热更新产物
/Assets/Game/Scripts/game.dll.bytes
/Assets/StreamingAssets/game.dll.bytes

# 用户设置
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D 自动生成
ExportedObj/
.consulo/
*.unitypackage

# Rider
.idea/

# Visual Studio
.vs/
```

### 应该提交的文件

- ✅ `.asmdef` 文件
- ✅ `.cs` 源代码
- ✅ `.meta` 文件
- ✅ `ProjectSettings/`
- ✅ `Packages/manifest.json`

---

## 最佳实践

### 1. 保持程序集独立

```csharp
// ✅ 正确：Framework 不依赖 Game
namespace Framework
{
    public class Hot : Singleton<Hot> { }
}

// ❌ 错误：Framework 依赖 Game
namespace Framework
{
    public class Hot : Singleton<Hot>
    {
        void Load()
        {
            Game.Data.Instance.Init();  // ❌ 不允许
        }
    }
}
```

### 2. 命名空间与目录对应

```csharp
// ✅ 正确：命名空间反映目录结构
// 文件：Assets/Game/Scripts/UI/Home/Home.cs
namespace Game.UI.Home
{
    public class Home : UIBase { }
}

// ❌ 错误：命名空间与目录不符
// 文件：Assets/Game/Scripts/UI/Home/Home.cs
namespace Game.Network  // ❌ 错误的命名空间
{
    public class Home : UIBase { }
}
```

### 3. 热更新代码放在 Game 程序集

```csharp
// ✅ 正确：业务逻辑在 Game 程序集
// 文件：Assets/Game/Scripts/Data/Data.cs
namespace Game
{
    public class Data : Singleton<Data> { }
}

// ❌ 错误：业务逻辑在 Framework 程序集
// 文件：Assets/Framework/Data.cs
namespace Framework
{
    public class Data : Singleton<Data> { }  // ❌ 不可热更新
}
```

---

## 总结

Unity项目结构规范定义了：

1. **程序集划分**：Assembly-CSharp、Framework、Game三层结构
2. **目录组织**：清晰的目录结构和职责划分
3. **命名空间约定**：命名空间反映目录结构
4. **文件归属**：明确各类文件的存放位置
5. **不可移动文件**：保护系统生成的关键文件

遵循这些规范可以：
- 保持项目结构清晰
- 支持热更新功能
- 提升团队协作效率
- 降低维护成本
